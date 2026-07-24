using System.IO.Compression;
using Bolt.Core.Abstractions;
using Bolt.Infrastructure.Storage;

namespace Bolt.Infrastructure.Archives;

/// <summary>
/// Reads <c>.zip</c> modification archives.
/// </summary>
/// <remarks>
/// The folder structure of the archive is reproduced exactly: a package whose files live inside a
/// folder keeps that folder, because only the author of the package knows where its files belong
/// inside the game. Bolt never rewrites the layout.
/// <para>
/// Every archive is opened inside a <c>using</c> block. The previous implementation leaked the
/// handle while counting entries, which kept the file locked for the rest of the session.
/// </para>
/// </remarks>
internal sealed class ZipArchiveReader : IArchiveReader
{
    public IReadOnlyCollection<string> SupportedExtensions { get; } = [".zip"];

    public bool CanRead(string archivePath) =>
        !string.IsNullOrWhiteSpace(archivePath)
        && SupportedExtensions.Contains(Path.GetExtension(archivePath), StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ArchiveEntry> ListEntries(string archivePath)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        return [.. archive.Entries
            .Where(IsFile)
            .Select(entry => new ArchiveEntry(entry.FullName, entry.Length))];
    }

    public IReadOnlyList<string> Extract(
        string archivePath,
        string destinationRoot,
        Action<string>? onEntryExtracted = null,
        CancellationToken cancellationToken = default)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        var fullRoot = Path.GetFullPath(destinationRoot);
        var extracted = new List<string>();

        Directory.CreateDirectory(fullRoot);

        foreach (var entry in archive.Entries.Where(IsFile))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = PathUtility.NormalizeRelative(entry.FullName);

            if (relativePath.Length == 0)
                continue;

            var destinationPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));

            // Guards against archives whose entries escape the destination folder ("zip slip").
            if (!PathUtility.IsInside(fullRoot, destinationPath))
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: true);

            extracted.Add(relativePath);
            onEntryExtracted?.Invoke(relativePath);
        }

        return extracted;
    }

    private static bool IsFile(ZipArchiveEntry entry) => !string.IsNullOrEmpty(entry.Name);
}
