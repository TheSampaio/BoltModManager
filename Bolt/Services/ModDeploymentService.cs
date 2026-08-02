using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Native;

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
        // Relative path -> owning modification. Overwriting keeps the last modification in profile
        // order as the winner, matching the conflict report shown to the user.
        var links = new Dictionary<string, Modification>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in enabled)
        {
            foreach (var relativePath in modification.Content)
                links[relativePath] = modification;
        }

        var restores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var modification in reverted)
        {
            foreach (var relativePath in modification.Content)
            {
                if (!links.ContainsKey(relativePath))
                    restores.Add(relativePath);
            }
        }

        var operations = new List<LinkOperation>(links.Count + restores.Count);

        foreach (var relativePath in restores)
        {
            var destinationPath = Path.Combine(session.Game.TargetPath, relativePath);
            var backupPath = Path.Combine(session.BackupsPath, relativePath);

            if (!SymbolicLink.IsLink(destinationPath) && !File.Exists(backupPath))
                continue;

            operations.Add(new LinkOperation
            {
                Action = LinkAction.Restore,
                DestinationPath = destinationPath,
                BackupPath = backupPath,
                CleanupRootPath = session.Game.TargetPath
            });
        }

        foreach (var (relativePath, modification) in links)
        {
            var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
            var destinationPath = Path.Combine(session.Game.TargetPath, relativePath);

            if (IsLinkedTo(destinationPath, sourcePath))
                continue;

            operations.Add(new LinkOperation
            {
                Action = LinkAction.Link,
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                BackupPath = Path.Combine(session.BackupsPath, relativePath)
            });
        }

        return operations;
    }

    /// <summary>True when <paramref name="path"/> already links to <paramref name="target"/>.</summary>
    private static bool IsLinkedTo(string path, string target)
    {
        // ResolveLinkTarget throws FileNotFoundException for every destination which has not been
        // deployed yet. Check the reparse-point attribute first so enabling a modification does
        // not use exceptions as the normal path for each new file.
        if (!SymbolicLink.IsLink(path))
            return false;

        try
        {
            return File.ResolveLinkTarget(path, returnFinalTarget: false) is { } linkTarget
                && Path.GetFullPath(linkTarget.FullName).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
