using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>Applies and removes modification files in the game folder.</summary>
internal interface IModDeploymentService
{
    /// <summary>
    /// Brings the game folder in sync with the active profile in a single batch: every enabled
    /// modification is linked and everything else is restored to its original state.
    /// </summary>
    /// <param name="removed">
    /// Modifications no longer present in the profile — deleted or replaced by a new import — whose
    /// files must be reverted as well. Files still claimed by an enabled modification stay linked.
    /// </param>
    OperationResult Synchronize(GameSession session, IReadOnlyCollection<Modification>? removed = null);

    /// <summary>
    /// Removes every Bolt-managed link, restores known backups, and disables all modifications.
    /// Files which Bolt did not deploy are deliberately left untouched.
    /// </summary>
    OperationResult RestoreDefaults(GameSession session);

    /// <summary>
    /// Relative paths provided by more than one enabled modification, mapped to the names of the
    /// modifications claiming them. The last one in profile order wins on disk.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> FindConflicts(GameSession session);
}
