using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;

namespace Bolt.Infrastructure.Deployment;

/// <summary>Errors reported back by the elevated helper process.</summary>
internal sealed class HelperReport
{
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Runs a manifest of link operations in a separate, elevated instance of the application.
/// </summary>
/// <remarks>
/// The helper never shows UI. It writes its outcome next to the manifest and exits with a status
/// code, so the parent process can report a real failure instead of assuming success.
/// </remarks>
internal static class ElevatedHelper
{
    public const string CommandLineSwitch = "--elevated-helper";

    public const int ExitSuccess = 0;
    public const int ExitOperationsFailed = 1;
    public const int ExitManifestUnreadable = 2;

    /// <summary>Path of the report written for <paramref name="manifestPath"/>.</summary>
    public static string GetReportPath(string manifestPath) => manifestPath + ".report";

    /// <summary>
    /// True when the process was started to act as the elevated helper.
    /// </summary>
    public static bool TryParseArguments(string[] args, out string manifestPath)
    {
        if (args.Length == 2 && args[0].Equals(CommandLineSwitch, StringComparison.Ordinal))
        {
            manifestPath = args[1];
            return true;
        }

        manifestPath = string.Empty;
        return false;
    }

    /// <summary>Entry point of the helper process. Returns the exit code of the batch.</summary>
    public static int Run(string manifestPath)
    {
        List<LinkOperation>? operations;

        try
        {
            operations = JsonFileStore.Read<List<LinkOperation>>(manifestPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            WriteReport(manifestPath, [$"The operation manifest could not be read: {ex.Message}"]);
            return ExitManifestUnreadable;
        }

        if (operations is null || operations.Count == 0)
        {
            WriteReport(manifestPath, []);
            return ExitSuccess;
        }

        var result = LinkOperationRunner.Run(operations);
        var errors = result.Errors.ToList();

        if (result.RequiresElevation)
            errors.Add("The operation was denied even with administrator rights.");

        WriteReport(manifestPath, errors);

        return errors.Count == 0 ? ExitSuccess : ExitOperationsFailed;
    }

    private static void WriteReport(string manifestPath, List<string> errors)
    {
        try
        {
            JsonFileStore.Write(new HelperReport { Errors = errors }, GetReportPath(manifestPath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Without a report the parent still sees the exit code, which is enough to warn.
        }
    }
}
