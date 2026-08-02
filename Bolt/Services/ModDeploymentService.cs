using System.Security.Cryptography;
using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Native;
using Bolt.Infrastructure.Storage;

namespace Bolt.Services;

/// <summary>
/// Turns the desired state of a profile into file system operations and applies them.
/// </summary>
/// <remarks>
/// A single planner builds every batch, so enabling, disabling, deleting and re-importing all
/// follow the exact same rules and need at most one elevation prompt. Operations that would not
/// change anything are dropped, which makes an unnecessary synchronisation completely free.
/// </remarks>
internal sealed class ModDeploymentService(ILinkOperationExecutor executor) : IModDeploymentService
{
    private readonly ILinkOperationExecutor _executor = executor;

    public OperationResult Synchronize(GameSession session, IReadOnlyCollection<Modification>? removed = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var modifications = session.ActiveProfile.Modifications;

        var reverted = modifications.Where(m => !m.IsEnabled);

        if (removed is { Count: > 0 })
            reverted = reverted.Concat(removed);

        return _executor.Apply(BuildPlan(session, modifications.Where(m => m.IsEnabled), reverted));
    }

    public OperationResult RestoreDefaults(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var modifications = session.Game.Profiles
            .SelectMany(profile => profile.Modifications)
            .ToList();
        var enabledStates = modifications.ToDictionary(modification => modification, modification => modification.IsEnabled);

        foreach (var modification in modifications)
            modification.IsEnabled = false;

        var result = _executor.Apply(BuildPlan(session, [], modifications));

        if (!result.Succeeded)
        {
            foreach (var (modification, wasEnabled) in enabledStates)
                modification.IsEnabled = wasEnabled;
        }

        return result;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> FindConflicts(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var owners = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in session.ActiveProfile.Modifications.Where(m => m.IsEnabled))
        {
            foreach (var relativePath in modification.Content)
            {
                if (!owners.TryGetValue(relativePath, out var claimants))
                    owners[relativePath] = claimants = [];

                claimants.Add(modification.Name);
            }
        }

        return owners
            .Where(pair => pair.Value.Count > 1)
            .ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds the operations that make the game folder match the requested state.
    /// </summary>
    /// <param name="enabled">Modifications whose files must be linked. Later entries win.</param>
    /// <param name="reverted">
    /// Modifications whose files must be unlinked, except where <paramref name="enabled"/> still
    /// claims them.
    /// </param>
    private static List<LinkOperation> BuildPlan(
        GameSession session,
        IEnumerable<Modification> enabled,
        IEnumerable<Modification> reverted)
    {
        var enabledList = enabled.ToList();
        var revertedList = reverted.DistinctBy(modification => modification.Id).ToList();

        // Relative path -> owning modification. Overwriting keeps the last modification in profile
        // order as the winner, matching the conflict report shown to the user.
        var links = new Dictionary<string, Modification>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in enabledList)
        {
            foreach (var relativePath in modification.Content)
                links[PathUtility.NormalizeRelative(relativePath)] = modification;
        }

        var restores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in revertedList)
        {
            foreach (var relativePath in modification.Content)
            {
                var normalizedPath = PathUtility.NormalizeRelative(relativePath);

                if (!links.ContainsKey(normalizedPath))
                    restores.Add(normalizedPath);
            }
        }

        var operations = new List<LinkOperation>(links.Count + restores.Count);
        var directoryCandidates = FindDirectoryCandidates(session, links);
        var candidateFiles = directoryCandidates
            .SelectMany(candidate => candidate.Files)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var restoredByDirectory = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var desiredDirectoryPaths = directoryCandidates.ToDictionary(
            candidate => candidate.RelativePath,
            StringComparer.OrdinalIgnoreCase);

        // Remove directory links created for an older desired state before planning their files.
        // This covers disabling a mod and splitting a previously isolated directory after a new
        // conflict appears.
        foreach (var modification in enabledList.Concat(revertedList).DistinctBy(item => item.Id))
        {
            foreach (var existing in FindSourceDirectoryMatches(
                session,
                modification,
                modification.Content.Select(PathUtility.NormalizeRelative)))
            {
                var destinationPath = Path.Combine(session.Game.TargetPath, existing.RelativePath);

                if (!IsDirectoryLinkedTo(destinationPath, existing.SourcePath))
                    continue;

                if (desiredDirectoryPaths.TryGetValue(existing.RelativePath, out var desired)
                    && desired.SourcePath.Equals(existing.SourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                operations.Add(new LinkOperation
                {
                    Action = LinkAction.RestoreDirectory,
                    DestinationPath = destinationPath,
                    CleanupRootPath = session.Game.TargetPath
                });
                restoredByDirectory.UnionWith(existing.Files);
            }
        }

        foreach (var candidate in directoryCandidates)
        {
            var destinationPath = Path.Combine(session.Game.TargetPath, candidate.RelativePath);

            if (IsDirectoryLinkedTo(destinationPath, candidate.SourcePath))
                continue;

            // Migrate the individual links and managed copies produced by earlier Bolt versions.
            // Candidate selection guarantees that none of these paths has an original backup.
            foreach (var relativePath in candidate.Files)
            {
                var fileDestination = Path.Combine(session.Game.TargetPath, relativePath);
                var statePath = GetMaterializationStatePath(session, relativePath);

                if (SymbolicLink.IsLink(fileDestination))
                {
                    operations.Add(new LinkOperation
                    {
                        Action = LinkAction.Restore,
                        DestinationPath = fileDestination,
                        BackupPath = Path.Combine(session.BackupsPath, relativePath),
                        CleanupRootPath = session.Game.TargetPath
                    });
                }
                else if (File.Exists(statePath))
                {
                    operations.Add(new LinkOperation
                    {
                        Action = LinkAction.RestoreMaterialized,
                        DestinationPath = fileDestination,
                        BackupPath = Path.Combine(session.BackupsPath, relativePath),
                        StatePath = statePath,
                        CleanupRootPath = session.Game.TargetPath
                    });
                }
            }

            operations.Add(new LinkOperation
            {
                Action = LinkAction.LinkDirectory,
                SourcePath = candidate.SourcePath,
                DestinationPath = destinationPath,
                CleanupRootPath = session.Game.TargetPath
            });
        }

        foreach (var relativePath in restores)
        {
            if (restoredByDirectory.Contains(relativePath))
                continue;

            var destinationPath = Path.Combine(session.Game.TargetPath, relativePath);
            var backupPath = Path.Combine(session.BackupsPath, relativePath);
            var statePath = GetMaterializationStatePath(session, relativePath);

            if (!SymbolicLink.IsLink(destinationPath)
                && !File.Exists(statePath)
                && !File.Exists(backupPath))
            {
                continue;
            }

            operations.Add(new LinkOperation
            {
                Action = SymbolicLink.IsLink(destinationPath)
                    ? LinkAction.Restore
                    : LinkAction.RestoreMaterialized,
                DestinationPath = destinationPath,
                BackupPath = backupPath,
                StatePath = statePath,
                CleanupRootPath = session.Game.TargetPath
            });
        }

        foreach (var (relativePath, modification) in links)
        {
            if (candidateFiles.Contains(relativePath))
                continue;

            var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
            var destinationPath = Path.Combine(session.Game.TargetPath, relativePath);
            var statePath = GetMaterializationStatePath(session, relativePath);

            if (IsMaterializedFrom(destinationPath, sourcePath, statePath))
                continue;

            operations.Add(new LinkOperation
            {
                Action = LinkAction.Materialize,
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                BackupPath = Path.Combine(session.BackupsPath, relativePath),
                StatePath = statePath
            });
        }

        return operations;
    }

    /// <summary>
    /// Finds isolated modification directories that can be mounted as one junction. Traversing a
    /// directory junction reports each target file's real metadata, while an individual Windows
    /// file symlink reports a length of zero to directory enumerators.
    /// </summary>
    private static List<DirectoryCandidate> FindDirectoryCandidates(
        GameSession session,
        IReadOnlyDictionary<string, Modification> links)
    {
        var candidates = new List<DirectoryCandidate>();
        var covered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in links.Values.DistinctBy(item => item.Id))
        {
            var ownedFiles = links
                .Where(pair => pair.Value.Id == modification.Id)
                .Select(pair => pair.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var match in FindSourceDirectoryMatches(session, modification, ownedFiles)
                .OrderBy(match => GetPathDepth(match.RelativePath))
                .ThenBy(match => match.RelativePath, StringComparer.OrdinalIgnoreCase))
            {
                if (match.Files.Any(covered.Contains)
                    || !CanReplaceDestinationDirectory(session, match))
                {
                    continue;
                }

                candidates.Add(match);
                covered.UnionWith(match.Files);
            }
        }

        return candidates;
    }

    private static IEnumerable<DirectoryCandidate> FindSourceDirectoryMatches(
        GameSession session,
        Modification modification,
        IEnumerable<string> desiredFiles)
    {
        var modificationPath = session.GetModificationPath(modification);
        var desired = desiredFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var physicalFiles = EnumeratePhysicalFiles(modificationPath);
        var directories = desired
            .SelectMany(GetParentDirectories)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var relativeDirectory in directories)
        {
            var desiredBelow = desired
                .Where(path => IsBelow(relativeDirectory, path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var physicalBelow = physicalFiles
                .Where(path => IsBelow(relativeDirectory, path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (desiredBelow.Count == 0 || !desiredBelow.SetEquals(physicalBelow))
                continue;

            yield return new DirectoryCandidate(
                relativeDirectory,
                Path.Combine(modificationPath, relativeDirectory),
                desiredBelow);
        }
    }

    private static HashSet<string> EnumeratePhysicalFiles(string rootPath)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(rootPath))
            return files;

        var pending = new Stack<string>();
        pending.Push(rootPath);

        while (pending.TryPop(out var directory))
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(entry);

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if ((attributes & FileAttributes.ReparsePoint) == 0)
                        pending.Push(entry);

                    continue;
                }

                files.Add(PathUtility.NormalizeRelative(Path.GetRelativePath(rootPath, entry)));
            }
        }

        return files;
    }

    private static bool CanReplaceDestinationDirectory(GameSession session, DirectoryCandidate candidate)
    {
        foreach (var relativePath in candidate.Files)
        {
            if (File.Exists(Path.Combine(session.BackupsPath, relativePath)))
                return false;
        }

        var destinationPath = Path.Combine(session.Game.TargetPath, candidate.RelativePath);

        if (File.Exists(destinationPath))
            return false;

        if (SymbolicLink.IsDirectoryLink(destinationPath))
            return IsDirectoryLinkedTo(destinationPath, candidate.SourcePath);

        var parent = Path.GetDirectoryName(destinationPath);

        while (parent is not null
            && PathUtility.IsInside(session.Game.TargetPath, parent)
            && !parent.Equals(session.Game.TargetPath, StringComparison.OrdinalIgnoreCase))
        {
            if (SymbolicLink.IsDirectoryLink(parent))
                return false;

            parent = Path.GetDirectoryName(parent);
        }

        if (!Directory.Exists(destinationPath))
            return true;

        var pending = new Stack<string>();
        pending.Push(destinationPath);

        while (pending.TryPop(out var directory))
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(entry);

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                        return false;

                    pending.Push(entry);
                    continue;
                }

                var relativePath = PathUtility.NormalizeRelative(
                    Path.GetRelativePath(session.Game.TargetPath, entry));

                if (!candidate.Files.Contains(relativePath)
                    || (!SymbolicLink.IsLink(entry)
                        && !File.Exists(GetMaterializationStatePath(session, relativePath))))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static IEnumerable<string> GetParentDirectories(string relativePath)
    {
        var directory = Path.GetDirectoryName(relativePath);

        while (!string.IsNullOrEmpty(directory))
        {
            yield return directory;
            directory = Path.GetDirectoryName(directory);
        }
    }

    private static bool IsBelow(string directory, string path) =>
        path.StartsWith(
            Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static int GetPathDepth(string path) =>
        path.Count(character => character is '\\' or '/');

    private static string GetMaterializationStatePath(GameSession session, string relativePath) =>
        Path.Combine(session.BackupsPath, ".bolt-state", $"{relativePath}.materialized");

    private static bool IsMaterializedFrom(string destinationPath, string sourcePath, string statePath)
    {
        if (!File.Exists(statePath)
            || SymbolicLink.IsLink(destinationPath)
            || !File.Exists(destinationPath)
            || !File.Exists(sourcePath))
        {
            return false;
        }

        try
        {
            var destination = new FileInfo(destinationPath);
            var source = new FileInfo(sourcePath);

            if (destination.Length != source.Length)
                return false;

            using var destinationStream = destination.OpenRead();
            using var sourceStream = source.OpenRead();

            return SHA256.HashData(destinationStream).AsSpan().SequenceEqual(SHA256.HashData(sourceStream));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsDirectoryLinkedTo(string path, string target)
    {
        if (!SymbolicLink.IsDirectoryLink(path))
            return false;

        try
        {
            return Directory.ResolveLinkTarget(path, returnFinalTarget: false) is { } linkTarget
                && Path.GetFullPath(linkTarget.FullName).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private sealed record DirectoryCandidate(
        string RelativePath,
        string SourcePath,
        HashSet<string> Files);
}
