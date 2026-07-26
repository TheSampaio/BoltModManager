using System.Runtime.InteropServices;

namespace Bolt.Infrastructure.Native;

/// <summary>Win32 entry points used by the application.</summary>
internal static partial class NativeMethods
{
    public const int SymbolicLinkFlagFile = 0x0;
    public const int SymbolicLinkFlagDirectory = 0x1;

    /// <summary>Allows symbolic link creation without elevation when Developer Mode is enabled.</summary>
    public const int SymbolicLinkFlagAllowUnprivilegedCreate = 0x2;

    public const int ErrorInvalidParameter = 87;
    public const int ErrorPrivilegeNotHeld = 1314;
    public const int ErrorAccessDenied = 5;
    public const int ErrorCancelled = 1223;

    public const uint InvalidFileAttributes = 0xFFFFFFFF;
    public const uint FileAttributeReparsePoint = 0x00000400;

    /// <summary>Enables the dark window frame introduced in Windows 10 20H1.</summary>
    public const int DwmwaUseImmersiveDarkMode = 20;

    /// <summary>Rounded corner preference of a window, honoured by Windows 11.</summary>
    public const int DwmwaWindowCornerPreference = 33;

    [LibraryImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool CreateSymbolicLink(string symlinkFileName, string targetFileName, int flags);

    [LibraryImport("kernel32.dll", EntryPoint = "GetFileAttributesW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint GetFileAttributes(string fileName);

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

    /// <summary>
    /// Switches a common control to a visual style. Passing <c>DarkMode_Explorer</c> gives native
    /// list views dark scroll bars, which cannot be reached through owner drawing.
    /// </summary>
    [LibraryImport("uxtheme.dll", EntryPoint = "SetWindowTheme", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int SetWindowTheme(IntPtr window, string subApplicationName, string? subIdList);

    public const int ListViewSetExtendedStyle = 0x1000 + 54;

    /// <summary>Makes a list view paint through an off-screen buffer, removing the flicker.</summary>
    public const int ListViewExtendedStyleDoubleBuffer = 0x00010000;

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
}
