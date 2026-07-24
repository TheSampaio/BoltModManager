using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Bolt.Infrastructure.Native;

/// <summary>
/// Creates symbolic links and reports why a creation failed.
/// </summary>
/// <remarks>
/// The previous wrapper discarded the Win32 error code, so a failed link looked exactly like a
/// successful one and mods appeared to deploy while nothing had changed on disk.
/// </remarks>
internal static class SymbolicLink
{
    private static bool? _unprivilegedCreationSupported;

    /// <summary>True when the process runs with an elevated administrator token.</summary>
    public static bool IsProcessElevated { get; } = DetectElevation();

    /// <summary>
    /// True when this machine lets the current process create links without elevation, either
    /// because it is already elevated or because Windows Developer Mode is enabled.
    /// </summary>
    public static bool CanCreateWithoutElevation => IsProcessElevated || SupportsUnprivilegedCreation();

    /// <summary>Creates a file symbolic link at <paramref name="linkPath"/>.</summary>
    /// <exception cref="Win32Exception">Creation failed; the message explains why.</exception>
    public static void CreateFileLink(string linkPath, string targetPath)
    {
        if (TryCreate(linkPath, targetPath, NativeMethods.SymbolicLinkFlagFile, out var error))
            return;

        throw new Win32Exception(error, DescribeError(error, linkPath));
    }

    /// <summary>True when the path exists and is a reparse point (symbolic link or junction).</summary>
    public static bool IsLink(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException)
        {
            return false;
        }
    }

    private static bool TryCreate(string linkPath, string targetPath, int flags, out int error)
    {
        if (NativeMethods.CreateSymbolicLink(linkPath, targetPath, flags | NativeMethods.SymbolicLinkFlagAllowUnprivilegedCreate))
        {
            error = 0;
            return true;
        }

        error = Marshal.GetLastWin32Error();

        // Windows builds older than 1703 reject the unprivileged flag outright: retry without it.
        if (error != NativeMethods.ErrorInvalidParameter)
            return false;

        if (NativeMethods.CreateSymbolicLink(linkPath, targetPath, flags))
        {
            error = 0;
            return true;
        }

        error = Marshal.GetLastWin32Error();
        return false;
    }

    private static string DescribeError(int error, string linkPath) => error switch
    {
        NativeMethods.ErrorPrivilegeNotHeld =>
            "Creating symbolic links requires administrator rights or Windows Developer Mode.",
        NativeMethods.ErrorAccessDenied =>
            $"Access to \"{linkPath}\" was denied.",
        _ => $"Failed to create the link \"{linkPath}\" (error {error})."
    };

    /// <summary>
    /// Probes Developer Mode by creating a throwaway link, caching the answer for the session.
    /// </summary>
    private static bool SupportsUnprivilegedCreation()
    {
        if (_unprivilegedCreationSupported.HasValue)
            return _unprivilegedCreationSupported.Value;

        var probeDirectory = Path.Combine(Path.GetTempPath(), $"BoltLinkProbe_{Guid.NewGuid():N}");
        var targetPath = Path.Combine(probeDirectory, "target");
        var linkPath = Path.Combine(probeDirectory, "link");

        try
        {
            Directory.CreateDirectory(probeDirectory);
            File.WriteAllText(targetPath, string.Empty);

            _unprivilegedCreationSupported = TryCreate(linkPath, targetPath, NativeMethods.SymbolicLinkFlagFile, out _);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _unprivilegedCreationSupported = false;
        }
        finally
        {
            TryDeleteDirectory(probeDirectory);
        }

        return _unprivilegedCreationSupported.Value;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A leftover probe folder in %TEMP% is harmless.
        }
    }

    private static bool DetectElevation()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
