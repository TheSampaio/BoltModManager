using System.Drawing;

namespace Bolt.UI.Theme;

/// <summary>
/// Semantic colour set of the application. Colours are referenced by role, never by literal, so a
/// scheme can be swapped without touching a single control.
/// </summary>
internal sealed record Palette
{
    public required bool IsDark { get; init; }

    /// <summary>Window background, behind every surface.</summary>
    public required Color Background { get; init; }

    /// <summary>Background of cards, panels and list rows.</summary>
    public required Color Surface { get; init; }

    /// <summary>Slightly raised surface used for headers, toolbars and alternating rows.</summary>
    public required Color SurfaceAlt { get; init; }

    public required Color SurfaceHover { get; init; }

    public required Color SurfaceActive { get; init; }

    public required Color Border { get; init; }

    public required Color BorderSubtle { get; init; }

    public required Color TextPrimary { get; init; }

    public required Color TextSecondary { get; init; }

    public required Color TextMuted { get; init; }

    public required Color Accent { get; init; }

    public required Color AccentHover { get; init; }

    public required Color AccentPressed { get; init; }

    /// <summary>Foreground used on top of <see cref="Accent"/>.</summary>
    public required Color OnAccent { get; init; }

    /// <summary>
    /// Accent tone for text, icons and outlines. The fill accent is deliberately deep, which makes
    /// it unreadable as a foreground on a dark surface, so those uses take this lighter tone.
    /// </summary>
    public required Color AccentText { get; init; }

    public required Color Success { get; init; }

    public required Color Warning { get; init; }

    public required Color Danger { get; init; }

    public required Color DangerHover { get; init; }

    /// <summary>Tint applied to the selected row of a list.</summary>
    public required Color Selection { get; init; }

    /// <summary>
    /// Default scheme: the two greys requested for the shell, with the violet of the product icon
    /// as the single accent.
    /// </summary>
    public static Palette Dark { get; } = new()
    {
        IsDark = true,
        Background = Color.FromArgb(0x17, 0x18, 0x1B),
        Surface = Color.FromArgb(0x1D, 0x1F, 0x21),
        SurfaceAlt = Color.FromArgb(0x25, 0x27, 0x2A),
        // Kept a clear step above SurfaceAlt: a hover the eye cannot see is a hover that does not
        // exist, which is exactly how the first dark pass failed.
        SurfaceHover = Color.FromArgb(0x35, 0x38, 0x3D),
        SurfaceActive = Color.FromArgb(0x42, 0x46, 0x4C),
        Border = Color.FromArgb(0x3C, 0x40, 0x46),
        BorderSubtle = Color.FromArgb(0x2B, 0x2E, 0x32),
        TextPrimary = Color.FromArgb(0xF2, 0xF4, 0xF8),
        TextSecondary = Color.FromArgb(0xB8, 0xBE, 0xC8),
        TextMuted = Color.FromArgb(0x8C, 0x93, 0x9E),
        Accent = Color.FromArgb(0x5F, 0x00, 0xA0),
        AccentHover = Color.FromArgb(0x77, 0x14, 0xBE),
        AccentPressed = Color.FromArgb(0x49, 0x00, 0x7C),
        OnAccent = Color.FromArgb(0xFF, 0xFF, 0xFF),
        AccentText = Color.FromArgb(0xB1, 0x7A, 0xE8),
        Success = Color.FromArgb(0x51, 0xCF, 0x8B),
        Warning = Color.FromArgb(0xEC, 0xBE, 0x50),
        Danger = Color.FromArgb(0xF0, 0x64, 0x5C),
        DangerHover = Color.FromArgb(0xF8, 0x7C, 0x74),
        Selection = Color.FromArgb(0x33, 0x22, 0x45)
    };

    public static Palette Light { get; } = new()
    {
        IsDark = false,
        Background = Color.FromArgb(0xF3, 0xF4, 0xF7),
        Surface = Color.FromArgb(0xFF, 0xFF, 0xFF),
        SurfaceAlt = Color.FromArgb(0xF7, 0xF8, 0xFA),
        SurfaceHover = Color.FromArgb(0xEC, 0xEC, 0xF3),
        SurfaceActive = Color.FromArgb(0xDF, 0xDF, 0xEC),
        Border = Color.FromArgb(0xD2, 0xD4, 0xDC),
        BorderSubtle = Color.FromArgb(0xE4, 0xE6, 0xEC),
        TextPrimary = Color.FromArgb(0x14, 0x15, 0x18),
        TextSecondary = Color.FromArgb(0x4A, 0x4F, 0x5A),
        TextMuted = Color.FromArgb(0x74, 0x7A, 0x87),
        Accent = Color.FromArgb(0x5F, 0x00, 0xA0),
        AccentHover = Color.FromArgb(0x74, 0x0F, 0xBC),
        AccentPressed = Color.FromArgb(0x49, 0x00, 0x7C),
        OnAccent = Color.FromArgb(0xFF, 0xFF, 0xFF),
        AccentText = Color.FromArgb(0x5F, 0x00, 0xA0),
        Success = Color.FromArgb(0x1D, 0x9B, 0x57),
        Warning = Color.FromArgb(0xB5, 0x7F, 0x0B),
        Danger = Color.FromArgb(0xC9, 0x3C, 0x2B),
        DangerHover = Color.FromArgb(0xDB, 0x4C, 0x3A),
        Selection = Color.FromArgb(0xEC, 0xE5, 0xFD)
    };
}
