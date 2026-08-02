using System.Buffers;
using Bolt.Core.Abstractions;
using Bolt.Infrastructure.Storage;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Bolt.Infrastructure.Archives;

/// <summary>Reads ZIP, 7-Zip, and RAR modification archives.</summary>
/// <remarks>
/// Archive entries are copied manually so every output path can be validated before data is
/// written. This keeps extraction independent of archive-specific path handling and prevents an
/// entry from escaping the modification folder.
/// </remarks>
internal sealed class ArchiveReader : IArchiveReader
{
    private const int CopyBufferSize = 1024 * 1024;
    private const int FileBufferSize = 64 * 1024;

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

    public int CountEntries(string archivePath, CancellationToken cancellationToken = default)
    {
        using var archive = ArchiveFactory.OpenArchive(new FileInfo(archivePath));
        var count = 0;

        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!entry.IsDirectory && !string.IsNullOrWhiteSpace(entry.Key))
                count++;
        }

        return count;
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
        var buffer = ArrayPool<byte>.Shared.Rent(CopyBufferSize);

        try
        {
            if (archive.IsSolid)
            {
                ExtractSolidArchive(
                    archive,
                    fullRoot,
                    extracted,
                    buffer,
                    onEntryExtracted,
                    cancellationToken);
            }
            else
            {
                ExtractRandomAccessArchive(
                    archive,
                    fullRoot,
                    extracted,
                    buffer,
                    onEntryExtracted,
                    cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return extracted;
    }

    private static void ExtractSolidArchive(
        IArchive archive,
        string destinationRoot,
        List<string> extracted,
        byte[] buffer,
        Action<string>? onEntryExtracted,
        CancellationToken cancellationToken)
    {
        using var reader = archive.ExtractAllEntries();

        while (reader.MoveToNextEntry())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.Entry.IsDirectory)
                continue;

            ExtractEntry(
                reader.Entry,
                reader.OpenEntryStream,
                destinationRoot,
                extracted,
                buffer,
                onEntryExtracted,
                cancellationToken);
        }
    }

    private static void ExtractRandomAccessArchive(
        IArchive archive,
        string destinationRoot,
        List<string> extracted,
        byte[] buffer,
        Action<string>? onEntryExtracted,
        CancellationToken cancellationToken)
    {
        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ExtractEntry(
                entry,
                entry.OpenEntryStream,
                destinationRoot,
                extracted,
                buffer,
                onEntryExtracted,
                cancellationToken);
        }
    }

    private static void ExtractEntry(
        IEntry entry,
        Func<Stream> openEntryStream,
        string destinationRoot,
        List<string> extracted,
        byte[] buffer,
        Action<string>? onEntryExtracted,
        CancellationToken cancellationToken)
    {
        var relativePath = PathUtility.NormalizeRelative(entry.Key ?? string.Empty);

        if (relativePath.Length == 0)
            return;

        var destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, relativePath));

        if (!PathUtility.IsInside(destinationRoot, destinationPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        using var source = openEntryStream();
        using var destination = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            FileBufferSize,
            FileOptions.None);

        CopyTo(source, destination, buffer, cancellationToken);

        extracted.Add(relativePath);
        onEntryExtracted?.Invoke(relativePath);
    }

    /// <summary>Copies one entry while retaining cancellation support for large files.</summary>
    private static void CopyTo(
        Stream source,
        Stream destination,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        int bytesRead;

        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            destination.Write(buffer, 0, bytesRead);
        }
    }
}
