using Bolt.Core.Abstractions;
using Bolt.Infrastructure.Storage;
using SharpCompress.Archives;

namespace Bolt.Infrastructure.Archives;

/// <summary>Reads ZIP, 7-Zip, and RAR modification archives.</summary>
/// <remarks>
/// Archive entries are copied manually so every output path can be validated before data is
/// written. This keeps extraction independent of archive-specific path handling and prevents an
/// entry from escaping the modification folder.
/// </remarks>
internal sealed class ArchiveReader : IArchiveReader
{
    private const int CopyBufferSize = 81920;

    public IReadOnlyCollection<string> SupportedExtensions { get; } = [".zip", ".7z", ".rar"];

    public bool CanRead(string archivePath) =>
        !string.IsNullOrWhiteSpace(archivePath)
        && SupportedExtensions.Contains(Path.GetExtension(archivePath), StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ArchiveEntry> ListEntries(string archivePath)
    {
        using var archive = ArchiveFactory.OpenArchive(new FileInfo(archivePath));

        return [.. archive.Entries
            .Where(entry => !entry.IsDirectory && !string.IsNullOrWhiteSpace(entry.Key))
            .Select(entry => new ArchiveEntry(entry.Key!, entry.Size))];
    }

    public IReadOnlyList<string> Extract(
        string archivePath,
        string destinationRoot,
        Action<string>? onEntryExtracted = null,
        CancellationToken cancellationToken = default)
    {
        using var archive = ArchiveFactory.OpenArchive(new FileInfo(archivePath));

        var fullRoot = Path.GetFullPath(destinationRoot);
        var extracted = new List<string>();

        Directory.CreateDirectory(fullRoot);

        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = PathUtility.NormalizeRelative(entry.Key ?? string.Empty);

            if (relativePath.Length == 0)
                continue;

            var destinationPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));

            if (!PathUtility.IsInside(fullRoot, destinationPath))
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            using var source = entry.OpenEntryStream();
            using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            CopyTo(source, destination, cancellationToken);

            extracted.Add(relativePath);
            onEntryExtracted?.Invoke(relativePath);
        }

        return extracted;
    }

    /// <summary>Copies one entry while retaining cancellation support for large files.</summary>
    private static void CopyTo(Stream source, Stream destination, CancellationToken cancellationToken)
    {
        var buffer = new byte[CopyBufferSize];
        int bytesRead;

        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            destination.Write(buffer, 0, bytesRead);
        }
    }
}
