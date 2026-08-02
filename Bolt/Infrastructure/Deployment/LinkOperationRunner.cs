using System.ComponentModel;
using Bolt.Core.Models;
using Bolt.Infrastructure.Native;
using Bolt.Infrastructure.Storage;

namespace Bolt.Infrastructure.Deployment;

/// <summary>Outcome of running a batch of link operations.</summary>
/// <param name="Errors">Human readable description of every operation that failed.</param>
/// <param name="RequiresElevation">
/// True when the batch stopped because the process lacked the rights to continue.
/// </param>
internal readonly record struct LinkRunResult(IReadOnlyList<string> Errors, bool RequiresElevation)
{
    public bool Succeeded => Errors.Count == 0 && !RequiresElevation;
}

/// <summary>
/// Applies link operations to the file system.
/// </summary>
/// <remarks>
/// Shared verbatim by the normal process and by the elevated helper, so both paths behave
/// identically. Every operation is written to be safely repeatable: re-running an aborted batch
/// with elevation can never overwrite a backup with a previously created link.
/// </remarks>
internal static class LinkOperationRunner
{
    public static LinkRunResult Run(IReadOnlyList<LinkOperation> operations)
    {
        var errors = new List<string>();

        foreach (var operation in operations)
        {
            try
            {
                Apply(operation);
            }
            catch (Exception ex) when (IsPermissionFailure(ex))
            {
                // Stop right away: continuing would leave the game folder half deployed. The caller
                // replays the whole batch with elevation instead.
                return new LinkRunResult(errors, RequiresElevation: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
            {
                errors.Add($"{operation.DestinationPath}: {ex.Message}");
            }
        }

        return new LinkRunResult(errors, RequiresElevation: false);
    }

    private static void Apply(LinkOperation operation)
    {
        switch (operation.Action)
        {
            case LinkAction.Link:
                Link(operation);
                break;

            case LinkAction.Restore:
                Restore(operation);
                break;

            default:
                throw new InvalidOperationException($"Unsupported link action \"{operation.Action}\".");
        }
    }

    private static void Link(LinkOperation operation)
    {
        if (!File.Exists(operation.SourcePath))
            throw new FileNotFoundException($"The modification file \"{operation.SourcePath}\" is missing.");

        Directory.CreateDirectory(Path.GetDirectoryName(operation.DestinationPath)!);

        if (SymbolicLink.IsLink(operation.DestinationPath))
        {
            // Replacing a link left by another modification: the original file is already backed up.
            File.Delete(operation.DestinationPath);
        }
        else if (File.Exists(operation.DestinationPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(operation.BackupPath)!);
            File.Move(operation.DestinationPath, operation.BackupPath, overwrite: true);
        }

        SymbolicLink.CreateFileLink(operation.DestinationPath, operation.SourcePath);
    }

    private static void Restore(LinkOperation operation)
    {
        if (SymbolicLink.IsLink(operation.DestinationPath))
            File.Delete(operation.DestinationPath);

        if (File.Exists(operation.BackupPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(operation.DestinationPath)!);
            File.Move(operation.BackupPath, operation.DestinationPath, overwrite: true);

            var backupRoot = FindBackupRoot(operation.BackupPath);

            if (backupRoot is not null)
                PathUtility.DeleteEmptyDirectories(Path.GetDirectoryName(operation.BackupPath)!, backupRoot);
        }

        PruneEmptyDestinationFolders(operation);
    }

    private static void PruneEmptyDestinationFolders(LinkOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.CleanupRootPath))
            return;

        var destinationFolder = Path.GetDirectoryName(operation.DestinationPath);

        if (destinationFolder is not null && PathUtility.IsInside(operation.CleanupRootPath, destinationFolder))
            PathUtility.DeleteEmptyDirectories(destinationFolder, operation.CleanupRootPath);
    }

    /// <summary>
    /// Walks up from a backup file to the folder named <c>Backups</c>, used as the boundary when
    /// pruning the empty folders left behind by a restore.
    /// </summary>
    private static string? FindBackupRoot(string backupPath)
    {
        var current = Path.GetDirectoryName(backupPath);

        while (!string.IsNullOrEmpty(current))
        {
            if (Path.GetFileName(current).Equals(GameSession.BackupsFolderName, StringComparison.OrdinalIgnoreCase))
                return current;

            current = Path.GetDirectoryName(current);
        }

        return null;
    }

    private static bool IsPermissionFailure(Exception exception) => exception switch
    {
        UnauthorizedAccessException => true,
        Win32Exception win32 => win32.NativeErrorCode is NativeMethods.ErrorAccessDenied
            or NativeMethods.ErrorPrivilegeNotHeld,
        _ => false
    };
}
