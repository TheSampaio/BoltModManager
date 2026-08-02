using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;

namespace Bolt.Services;

/// <summary>Edits modification metadata and reorganizes its managed files.</summary>
/// <remarks>
/// File moves are staged before their final destinations are populated. This supports swaps and
/// keeps the original layout recoverable if a move or deployment operation fails.
/// </remarks>
internal sealed class ModEditorService(IModDeploymentService deployment, IGameRepository repository) : IModEditorService
{
    private readonly IModDeploymentService _deployment = deployment;
    private readonly IGameRepository _repository = repository;

    public OperationResult Apply(GameSession session, Modification modification, ModificationEdit edit)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(modification);
        ArgumentNullException.ThrowIfNull(edit);

        var validation = Validate(session, modification, edit);

        if (validation.Error is not null)
            return OperationResult.Failure(validation.Error);

        var snapshot = Clone(modification);
        var moves = validation.Files
            .Where(file => !file.SourcePath.Equals(file.DestinationPath, StringComparison.Ordinal))
            .ToList();

        if (moves.Count == 0 && validation.RemovedFiles.Count == 0)
        {
            ApplyMetadata(modification, edit);

            try
            {
                _repository.Save(session);
                return OperationResult.Success();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Restore(modification, snapshot);
                return OperationResult.Failure($"The modification could not be saved: {ex.Message}");
            }
        }

        if (snapshot.IsEnabled)
        {
            modification.IsEnabled = false;

            var undeploy = _deployment.Synchronize(session, [snapshot]);

            if (undeploy.Failed)
            {
                modification.IsEnabled = true;
                return undeploy;
            }
        }

        var transaction = new FileMoveTransaction(
            session.GetModificationPath(modification),
            session.ModificationsPath,
            moves);

        try
        {
            transaction.Apply();
            ApplyMetadata(modification, edit);
            modification.Content = [.. validation.Files.Select(file => file.DestinationPath)];
            modification.IsEnabled = snapshot.IsEnabled;

            if (snapshot.IsEnabled)
            {
                var redeploy = _deployment.Synchronize(session, [snapshot]);

                if (redeploy.Failed)
                    return RollBack(session, modification, snapshot, transaction, redeploy.Error);
            }

            _repository.Save(session);
            transaction.Complete();
            return OperationResult.Success();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return RollBack(session, modification, snapshot, transaction, ex.Message);
        }
    }

    private OperationResult RollBack(
        GameSession session,
        Modification modification,
        Modification snapshot,
        FileMoveTransaction transaction,
        string? cause)
    {
        string? rollbackError = null;
        var failedState = Clone(modification);

        try
        {
            transaction.RollBack();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            rollbackError = ex.Message;
        }

        Restore(modification, snapshot);

        var deployment = snapshot.IsEnabled
            ? _deployment.Synchronize(session, [failedState])
            : OperationResult.Success();

        var message = $"The modification could not be edited: {cause ?? "Unknown error."}";

        if (rollbackError is not null || deployment.Failed)
        {
            message += Environment.NewLine
                + $"Rollback needs attention: {rollbackError ?? deployment.Error}";
        }

        return OperationResult.Failure(message);
    }

    private static (string? Error, IReadOnlyList<ModFileEdit> Files, IReadOnlyList<string> RemovedFiles) Validate(
        GameSession session,
        Modification modification,
        ModificationEdit edit)
    {
        if (string.IsNullOrWhiteSpace(edit.Name))
            return ("Enter a name for the modification.", [], []);

        if (session.ActiveProfile.Modifications.Any(candidate =>
            candidate.Id != modification.Id
            && candidate.Name.Equals(edit.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return ($"A modification named \"{edit.Name.Trim()}\" already exists in this profile.", [], []);
        }

        if (edit.Files.Count > modification.Content.Count)
            return ("The file list changed while the editor was open. Reopen it and try again.", [], []);

        var modificationRoot = Path.GetFullPath(session.GetModificationPath(modification));
        var expectedSources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var destinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validated = new List<ModFileEdit>(edit.Files.Count);
        var requestedFiles = new List<(ModFileEdit File, string SourcePath, string SourceFullPath)>(edit.Files.Count);

        foreach (var contentPath in modification.Content)
        {
            if (!TryResolveRelativeFile(modificationRoot, contentPath, out var sourcePath, out var sourceFullPath)
                || !expectedSources.TryAdd(sourcePath, sourceFullPath))
            {
                return ("The modification contains an invalid source file list.", [], []);
            }

            if (!File.Exists(sourceFullPath))
                return ($"The source file \"{sourcePath}\" no longer exists.", [], []);
        }

        foreach (var file in edit.Files)
        {
            if (!TryResolveRelativeFile(modificationRoot, file.SourcePath, out var sourcePath, out var sourceFullPath)
                || !expectedSources.ContainsKey(sourcePath)
                || !sources.Add(sourcePath))
            {
                return ("The source file list is invalid. Reopen the editor and try again.", [], []);
            }

            if (!File.Exists(sourceFullPath))
                return ($"The source file \"{sourcePath}\" no longer exists.", [], []);

            requestedFiles.Add((file, sourcePath, sourceFullPath));
        }

        foreach (var requested in requestedFiles)
        {
            if (!TryResolveRelativeFile(
                modificationRoot,
                requested.File.DestinationPath,
                out var destinationPath,
                out var destinationFullPath))
            {
                return ($"\"{requested.File.DestinationPath}\" is not a valid relative file path.", [], []);
            }

            if (!destinations.Add(destinationPath))
                return ($"More than one file uses the destination \"{destinationPath}\".", [], []);

            var destinationIsRetainedSource = requestedFiles.Any(candidate =>
                candidate.SourceFullPath.Equals(destinationFullPath, StringComparison.OrdinalIgnoreCase));

            if (File.Exists(destinationFullPath) && !destinationIsRetainedSource)
                return ($"The destination \"{destinationPath}\" is already occupied by another file.", [], []);

            validated.Add(new ModFileEdit(requested.SourcePath, destinationPath));
        }

        var removedFiles = expectedSources.Keys
            .Where(source => !sources.Contains(source))
            .ToList();

        return (null, validated, removedFiles);
    }

    private static bool TryResolveRelativeFile(
        string root,
        string candidate,
        out string relativePath,
        out string fullPath)
    {
        relativePath = string.Empty;
        fullPath = string.Empty;

        if (string.IsNullOrWhiteSpace(candidate)
            || Path.IsPathFullyQualified(candidate)
            || Path.EndsInDirectorySeparator(candidate.Trim()))
            return false;

        try
        {
            var normalizedInput = candidate.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            fullPath = Path.GetFullPath(Path.Combine(root, normalizedInput));

            if (!PathUtility.IsInside(root, fullPath)
                || fullPath.Equals(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(Path.GetFileName(fullPath)))
            {
                return false;
            }

            relativePath = Path.GetRelativePath(root, fullPath);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static void ApplyMetadata(Modification modification, ModificationEdit edit)
    {
        modification.Name = edit.Name.Trim();
        modification.Description = edit.Description.Trim();
        modification.Version = edit.Version.Trim();
        modification.Category = edit.Category.Trim();
    }

    private static Modification Clone(Modification source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Description = source.Description,
        Version = source.Version,
        Category = source.Category,
        InstalledAt = source.InstalledAt,
        IsEnabled = source.IsEnabled,
        FolderName = source.FolderName,
        Content = [.. source.Content]
    };

    private static void Restore(Modification target, Modification source)
    {
        target.Name = source.Name;
        target.Description = source.Description;
        target.Version = source.Version;
        target.Category = source.Category;
        target.InstalledAt = source.InstalledAt;
        target.IsEnabled = source.IsEnabled;
        target.FolderName = source.FolderName;
        target.Content = [.. source.Content];
    }

    /// <summary>Moves a validated set of files through an isolated staging folder.</summary>
    private sealed class FileMoveTransaction
    {
        private readonly string _modificationRoot;
        private readonly string _stagingRoot;
        private readonly List<Move> _moves;
        private readonly List<Move> _staged = [];
        private readonly List<Move> _placed = [];

        private bool _wasApplied;

        public FileMoveTransaction(
            string modificationRoot,
            string modificationsRoot,
            IReadOnlyList<ModFileEdit> files)
        {
            _modificationRoot = Path.GetFullPath(modificationRoot);
            _stagingRoot = Path.Combine(modificationsRoot, $".bolt-edit-{Guid.NewGuid():N}");
            _moves = [.. files.Select((file, index) => new Move(
                Path.Combine(_modificationRoot, file.SourcePath),
                Path.Combine(_modificationRoot, file.DestinationPath),
                Path.Combine(_stagingRoot, index.ToString(System.Globalization.CultureInfo.InvariantCulture))))];
        }

        public void Apply()
        {
            Directory.CreateDirectory(_stagingRoot);

            foreach (var move in _moves)
            {
                File.Move(move.Source, move.Staging);
                _staged.Add(move);
            }

            foreach (var move in _moves)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(move.Destination)!);
                File.Move(move.Staging, move.Destination);
                _staged.Remove(move);
                _placed.Add(move);
            }

            _wasApplied = true;
        }

        public void RollBack()
        {
            Directory.CreateDirectory(_stagingRoot);

            foreach (var move in _placed.AsEnumerable().Reverse())
            {
                File.Move(move.Destination, move.Staging);
                _staged.Add(move);
            }

            foreach (var move in _staged.AsEnumerable().Reverse())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(move.Source)!);
                File.Move(move.Staging, move.Source);
            }

            _placed.Clear();
            _staged.Clear();

            foreach (var destinationDirectory in _moves
                .Select(move => Path.GetDirectoryName(move.Destination)!)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                PathUtility.DeleteEmptyDirectories(destinationDirectory, _modificationRoot);
            }

            CleanUp();
        }

        public void Complete()
        {
            if (!_wasApplied)
                return;

            try
            {
                foreach (var sourceDirectory in _moves
                    .Select(move => Path.GetDirectoryName(move.Source)!)
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    PathUtility.DeleteEmptyDirectories(sourceDirectory, _modificationRoot);
                }

                CleanUp();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The edited state is already deployed and persisted. A locked staging file can be
                // cleaned up later without making a successful edit appear to have failed.
            }
        }

        private void CleanUp()
        {
            if (Directory.Exists(_stagingRoot))
                Directory.Delete(_stagingRoot, recursive: true);
        }

        private sealed record Move(string Source, string Destination, string Staging);
    }
}
