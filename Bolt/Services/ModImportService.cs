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
    private const int MaxConcurrentArchives = 2;

    private readonly IArchiveReader _archiveReader = archiveReader;

    public IReadOnlyCollection<string> SupportedExtensions => _archiveReader.SupportedExtensions;

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

        var workItems = archivePaths
            .Where(_archiveReader.CanRead)
            .Select((archivePath, index) => CreateWorkItem(archivePath, session.ModificationsPath, index))
            .ToList();

        var total = await CountEntriesAsync(workItems, cancellationToken).ConfigureAwait(false);
        var completed = 0;

        try
        {
            await Parallel.ForEachAsync(
                workItems,
                CreateParallelOptions(cancellationToken),
                (workItem, token) =>
                {
                    workItem.Content = _archiveReader.Extract(
                        workItem.ArchivePath,
                        workItem.TemporaryPath,
                        entry =>
                        {
                            var current = Interlocked.Increment(ref completed);
                            progress?.Report(new ImportProgress(current, total, entry));
                        },
                        token);

                    return ValueTask.CompletedTask;
                }).ConfigureAwait(false);

            // Committing consists only of local folder moves. Cancellation is accepted before the
            // first move so an import can never leave a partially updated profile.
            cancellationToken.ThrowIfCancellationRequested();
            return CommitImports(workItems, profile);
        }
        finally
        {
            foreach (var workItem in workItems)
                DeleteTemporaryFolder(workItem.TemporaryPath);
        }
    }

    /// <summary>
    /// Reads only archive metadata so the UI can provide determinate progress without
    /// decompressing any file twice.
    /// </summary>
    private async Task<int> CountEntriesAsync(
        IReadOnlyList<ImportWorkItem> workItems,
        CancellationToken cancellationToken)
    {
        var total = 0;

        await Parallel.ForEachAsync(
            workItems,
            CreateParallelOptions(cancellationToken),
            (workItem, token) =>
            {
                Interlocked.Add(ref total, _archiveReader.CountEntries(workItem.ArchivePath, token));
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);

        return total;
    }

    private static List<ImportedMod> CommitImports(
        IEnumerable<ImportWorkItem> workItems,
        Profile profile)
    {
        var imported = new List<ImportedMod>();

        foreach (var workItem in workItems.OrderBy(item => item.Index))
        {
            var replaced = TakeExisting(profile, workItem.Name, workItem.FolderName);
            ReplaceFolder(workItem.TemporaryPath, workItem.DestinationPath);

            var modification = new Modification
            {
                Name = workItem.Name,
                FolderName = workItem.FolderName,
                Description = replaced?.Description ?? string.Empty,
                Version = replaced?.Version ?? string.Empty,
                Category = replaced?.Category ?? string.Empty,
                InstalledAt = DateTime.Now,
                IsEnabled = replaced?.IsEnabled ?? true,
                Content = [.. workItem.Content]
            };

            profile.Modifications.Add(modification);
            imported.Add(new ImportedMod(modification, replaced));
        }

        return imported;
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

    private static ParallelOptions CreateParallelOptions(CancellationToken cancellationToken) => new()
    {
        CancellationToken = cancellationToken,
        MaxDegreeOfParallelism = MaxConcurrentArchives
    };

    private static ImportWorkItem CreateWorkItem(string archivePath, string modificationsPath, int index)
    {
        var name = Path.GetFileNameWithoutExtension(archivePath);
        var folderName = PathUtility.ToSafeFolderName(name);

        return new ImportWorkItem(
            index,
            archivePath,
            name,
            folderName,
            Path.Combine(modificationsPath, folderName),
            Path.Combine(modificationsPath, $".bolt-import-{Guid.NewGuid():N}"));
    }

    private static void ReplaceFolder(string sourcePath, string destinationPath)
    {
        if (Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, recursive: true);

        Directory.Move(sourcePath, destinationPath);
    }

    private static void DeleteTemporaryFolder(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record ImportWorkItem(
        int Index,
        string ArchivePath,
        string Name,
        string FolderName,
        string DestinationPath,
        string TemporaryPath)
    {
        public IReadOnlyList<string> Content { get; set; } = [];
    }
}
