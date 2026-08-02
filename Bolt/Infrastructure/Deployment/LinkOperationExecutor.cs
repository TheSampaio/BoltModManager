using System.ComponentModel;
using System.Diagnostics;
using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Native;
using Bolt.Infrastructure.Storage;

namespace Bolt.Infrastructure.Deployment;

/// <summary>
/// Executes link operations, asking for elevation only when it is actually required.
/// </summary>
/// <remarks>
/// Games installed outside protected folders, or machines with Developer Mode enabled, never see
/// a UAC prompt: the batch runs in process and only falls back to the helper when Windows denies
/// an operation.
/// </remarks>
internal sealed class LinkOperationExecutor : ILinkOperationExecutor
{
    private static readonly TimeSpan HelperTimeout = TimeSpan.FromMinutes(10);

    public OperationResult Apply(IReadOnlyList<LinkOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);

        if (operations.Count == 0)
            return OperationResult.Success();

        var requiresFileSymbolicLink = operations.Any(operation => operation.Action == LinkAction.Link);

        if (!requiresFileSymbolicLink || SymbolicLink.CanCreateWithoutElevation)
        {
            var result = LinkOperationRunner.Run(operations);

            if (!result.RequiresElevation)
            {
                return result.Errors.Count == 0
                    ? OperationResult.Success()
                    : OperationResult.Failure(Describe(result.Errors));
            }
        }

        return ApplyElevated(operations);
    }

    private static OperationResult ApplyElevated(IReadOnlyList<LinkOperation> operations)
    {
        var manifestPath = Path.Combine(Path.GetTempPath(), $"BoltManifest_{Guid.NewGuid():N}.json");
        var reportPath = ElevatedHelper.GetReportPath(manifestPath);

        try
        {
            JsonFileStore.Write(operations, manifestPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Application.ExecutablePath,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            startInfo.ArgumentList.Add(ElevatedHelper.CommandLineSwitch);
            startInfo.ArgumentList.Add(manifestPath);

            using var process = Process.Start(startInfo);

            if (process is null)
                return OperationResult.Failure("The elevated helper process could not be started.");

            if (!process.WaitForExit((int)HelperTimeout.TotalMilliseconds))
                return OperationResult.Failure("The elevated helper did not finish in time.");

            if (process.ExitCode == ElevatedHelper.ExitSuccess)
                return OperationResult.Success();

            var report = JsonFileStore.ReadOrDefault(reportPath, () => new HelperReport());

            return OperationResult.Failure(report.Errors.Count > 0
                ? Describe(report.Errors)
                : "The elevated helper failed to apply the modifications.");
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == NativeMethods.ErrorCancelled)
        {
            return OperationResult.Canceled("Administrator rights are required to apply the modifications.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
        {
            return OperationResult.Failure($"The modifications could not be applied: {ex.Message}");
        }
        finally
        {
            TryDelete(manifestPath);
            TryDelete(reportPath);
        }
    }

    private static string Describe(IReadOnlyList<string> errors)
    {
        const int maxListed = 8;

        var listed = string.Join(Environment.NewLine, errors.Take(maxListed).Select(e => $"• {e}"));

        return errors.Count > maxListed
            ? $"{listed}{Environment.NewLine}… and {errors.Count - maxListed} more."
            : listed;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Temporary files are cleaned up by Windows; failing to delete them is not an error.
        }
    }
}
