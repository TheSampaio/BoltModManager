namespace Bolt.Core.Models;

/// <summary>Kind of file system change described by a <see cref="LinkOperation"/>.</summary>
internal enum LinkAction
{
    /// <summary>Legacy file-link deployment retained so older deployments can be migrated safely.</summary>
    Link,

    /// <summary>Remove the link and restore the original file from the backup folder.</summary>
    Restore,

    /// <summary>
    /// Back up the original file and copy the modification into the game folder when the consumer
    /// requires local path or file metadata that a symbolic link cannot preserve.
    /// </summary>
    Materialize,

    /// <summary>Remove a materialized file and restore its original file, if one existed.</summary>
    RestoreMaterialized,

    /// <summary>Expose an external modification directory inside the game through a junction.</summary>
    LinkDirectory,

    /// <summary>Remove a directory junction previously created by Bolt.</summary>
    RestoreDirectory
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

    /// <summary>
    /// Marker proving that a regular destination file is managed by Bolt, so repeated
    /// synchronization cannot mistake the deployed copy for the original.
    /// </summary>
    public string StatePath { get; set; } = string.Empty;

    /// <summary>
    /// Boundary used when pruning empty destination folders after a restore. The boundary itself
    /// is never removed.
    /// </summary>
    public string CleanupRootPath { get; set; } = string.Empty;
}
