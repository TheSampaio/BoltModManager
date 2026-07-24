namespace Bolt.Core.Models;

/// <summary>Kind of file system change described by a <see cref="LinkOperation"/>.</summary>
internal enum LinkAction
{
    /// <summary>Back up the original file (if any) and link the modification file in its place.</summary>
    Link,

    /// <summary>Remove the link and restore the original file from the backup folder.</summary>
    Restore
}

/// <summary>
/// A single file system change. Batches of operations form the manifest handed to the elevated
/// helper process, which is why this type must stay serialization friendly.
/// </summary>
internal sealed class LinkOperation
{
    public LinkAction Action { get; set; }

    public string SourcePath { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    public string BackupPath { get; set; } = string.Empty;
}
