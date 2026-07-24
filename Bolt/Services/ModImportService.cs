using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;

namespace Bolt.Services;

/// <summary>
/// Imports modification archives into a profile.
/// </summary>
/// <remarks>
/// Re-importing a modification wipes its previous folder first. Leaving the old files in place
/// used to strand links pointing at files the new version no longer provides.
/// </remarks>
internal sealed class ModImportService(IArchiveReader archiveReader) : IModImportService
{
    private readonly IArchiveReader _archiveReader = archiveReader;

    public IReadOnlyCollection<string> SupportedExtensions => _archiveReader.SupportedExtensions;

    public Task<int> CountEntriesAsync(IReadOnlyList<string> archivePaths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archivePaths);

        return Task.Run(() =>
        {
            var total = 0;

            foreach (var archivePath in archivePaths.Where(_archiveReader.CanRead))
            {
                cancellationToken.ThrowIfCancellationRequested();
                total += _archiveReader.ListEntries(archivePath).Count;
            }

            return total;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<ImportedMod>> ImportAsync(
        IReadOnlyList<string> archivePaths,
        GameSession session,
        Profile profile,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archivePaths);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(profile);

        var total = await CountEntriesAsync(archivePaths, cancellationToken).ConfigureAwait(false);

        return await Task.Run(() =>
        {
            var imported = new List<ImportedMod>();
            var completed = 0;

            foreach (var archivePath in archivePaths.Where(_archiveReader.CanRead))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var name = Path.GetFileNameWithoutExtension(archivePath);
                var folderName = PathUtility.ToSafeFolderName(name);
                var destination = Path.Combine(session.ModificationsPath, folderName);

                var replaced = TakeExisting(profile, name, folderName);

                ResetFolder(destination);

                var content = _archiveReader.Extract(
                    archivePath,
                    destination,
                    entry =>
                    {
                        completed++;
                        progress?.Report(new ImportProgress(completed, total, entry));
                    },
                    cancellationToken);

                var modification = new Modification
                {
                    Name = name,
                    FolderName = folderName,
                    Version = replaced?.Version ?? string.Empty,
                    Category = replaced?.Category ?? string.Empty,
                    InstalledAt = DateTime.Now,
                    IsEnabled = replaced?.IsEnabled ?? true,
                    Content = [.. content]
                };

                profile.Modifications.Add(modification);
                imported.Add(new ImportedMod(modification, replaced));
            }

            return (IReadOnlyList<ImportedMod>)imported;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Removes a previous version of the same modification and returns it.</summary>
    private static Modification? TakeExisting(Profile profile, string name, string folderName)
    {
        var index = profile.Modifications.FindIndex(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            || m.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            return null;

        var existing = profile.Modifications[index];
        profile.Modifications.RemoveAt(index);

        return existing;
    }

    private static void ResetFolder(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
