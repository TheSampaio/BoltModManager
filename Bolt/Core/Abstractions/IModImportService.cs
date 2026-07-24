using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>Progress of an ongoing import, expressed in extracted entries.</summary>
internal readonly record struct ImportProgress(int Completed, int Total, string CurrentItem);

/// <summary>
/// One imported archive.
/// </summary>
/// <param name="Modification">The modification added to the profile.</param>
/// <param name="Replaced">
/// The previous version of the same modification, already removed from the profile. Its file list
/// must be handed to the deployment service so files it no longer provides are restored.
/// </param>
internal readonly record struct ImportedMod(Modification Modification, Modification? Replaced);

/// <summary>Extracts modification archives into a profile.</summary>
internal interface IModImportService
{
    /// <summary>Archive extensions accepted by <see cref="ImportAsync"/>, including the dot.</summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>Counts the entries of <paramref name="archivePaths"/> for progress reporting.</summary>
    Task<int> CountEntriesAsync(IReadOnlyList<string> archivePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts every archive into the modifications folder and adds the results to
    /// <paramref name="profile"/>. Files are only linked into the game when the caller
    /// synchronises afterwards.
    /// </summary>
    Task<IReadOnlyList<ImportedMod>> ImportAsync(
        IReadOnlyList<string> archivePaths,
        GameSession session,
        Profile profile,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
