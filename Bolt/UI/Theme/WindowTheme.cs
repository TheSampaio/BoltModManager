using Bolt.Infrastructure.Native;

namespace Bolt.UI.Theme;

/// <summary>Applies the palette to the non-client area of a window.</summary>
internal static class WindowTheme
{
    private const int RoundedCorners = 2;

    /// <summary>
    /// Matches the title bar to the active theme. Silently does nothing on Windows builds that do
    /// not expose the attribute.
    /// </summary>
    public static void Apply(Form form)
    {
        if (!form.IsHandleCreated)
            return;

        var useDarkMode = AppTheme.Colors.IsDark ? 1 : 0;
        NativeMethods.DwmSetWindowAttribute(form.Handle, NativeMethods.DwmwaUseImmersiveDarkMode, ref useDarkMode, sizeof(int));

        var cornerPreference = RoundedCorners;
        NativeMethods.DwmSetWindowAttribute(form.Handle, NativeMethods.DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));
    }
}
