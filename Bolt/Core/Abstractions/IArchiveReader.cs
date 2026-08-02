namespace Bolt.Core.Abstractions;

/// <summary>A single file inside an archive.</summary>
internal readonly record struct ArchiveEntry(string RelativePath, long Length);

/// <summary>Reads modification archives. Implementations decide which formats they support.</summary>
internal interface IArchiveReader
{
    /// <summary>Extensions handled by this reader, including the leading dot and lower-cased.</summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    bool CanRead(string archivePath);

    /// <summary>Lists the file entries of the archive, skipping directory entries.</summary>
    IReadOnlyList<ArchiveEntry> ListEntries(string archivePath);

    /// <summary>Counts file entries without reading their decompressed contents.</summary>
    int CountEntries(string archivePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts <paramref name="archivePath"/> into <paramref name="destinationRoot"/> and returns
    /// the extracted files as paths relative to that root.
    /// </summary>
    IReadOnlyList<string> Extract(
        string archivePath,
        string destinationRoot,
        Action<string>? onEntryExtracted = null,
        CancellationToken cancellationToken = default);
}
