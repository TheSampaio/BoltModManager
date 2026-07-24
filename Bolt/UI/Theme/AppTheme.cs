using System.Drawing;
using Bolt.Core.Models;
using Microsoft.Win32;

namespace Bolt.UI.Theme;

/// <summary>
/// Central access point for colours, typography and spacing.
/// </summary>
/// <remarks>
/// Controls read every visual value from here instead of hard-coding <c>SystemColors</c>, which is
/// what allows the whole application to follow one consistent, modern scheme.
/// </remarks>
internal static class AppTheme
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    private static readonly string[] PreferredFamilies =
        ["Segoe UI Variable Text", "Segoe UI", "Inter", "Tahoma"];

    static AppTheme()
    {
        FontFamilyName = ResolveFontFamily();
        Colors = Palette.Dark;
    }

    /// <summary>Colours currently in use.</summary>
    public static Palette Colors { get; private set; }

    public static string FontFamilyName { get; }

    /// <summary>Applies <paramref name="mode"/>, resolving <see cref="ThemeMode.System"/>.</summary>
    public static void Apply(ThemeMode mode) => Colors = mode switch
    {
        ThemeMode.Light => Palette.Light,
        ThemeMode.Dark => Palette.Dark,
        _ => IsSystemUsingDarkMode() ? Palette.Dark : Palette.Light
    };

    public static bool IsSystemUsingDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            return key?.GetValue(AppsUseLightThemeValue) is int value && value == 0;
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return true;
        }
    }

    /// <summary>Consistent spacing steps, in device independent pixels.</summary>
    internal static class Spacing
    {
        public const int Tiny = 4;
        public const int Small = 8;
        public const int Medium = 12;
        public const int Large = 16;
        public const int XLarge = 24;
        public const int XXLarge = 32;
    }

    /// <summary>Corner radii used across the interface.</summary>
    internal static class Radius
    {
        public const int Small = 6;
        public const int Medium = 8;
        public const int Large = 12;
    }

    /// <summary>Type scale of the application.</summary>
    internal static class Fonts
    {
        public static Font Title { get; } = Create(18f, FontStyle.Regular);

        public static Font Subtitle { get; } = Create(13f, FontStyle.Regular);

        public static Font Heading { get; } = Create(10.5f, FontStyle.Bold);

        public static Font Body { get; } = Create(9.75f, FontStyle.Regular);

        public static Font BodyStrong { get; } = Create(9.75f, FontStyle.Bold);

        public static Font Caption { get; } = Create(8.5f, FontStyle.Regular);

        /// <summary>Small upper-case label used for section headers.</summary>
        public static Font Overline { get; } = Create(8f, FontStyle.Bold);

        private static Font Create(float size, FontStyle style) =>
            new(FontFamilyName, size, style, GraphicsUnit.Point);
    }

    private static string ResolveFontFamily()
    {
        using var installed = new System.Drawing.Text.InstalledFontCollection();

        var available = installed.Families.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return PreferredFamilies.FirstOrDefault(available.Contains) ?? FontFamily.GenericSansSerif.Name;
    }
}
