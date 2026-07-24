using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bolt.UI.Theme;

/// <summary>Drawing helpers shared by the custom controls.</summary>
internal static class GraphicsExtensions
{
    /// <summary>Builds a rounded rectangle path. A radius of zero yields a plain rectangle.</summary>
    public static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);

        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();

        return path;
    }

    public static void FillRoundedRectangle(this Graphics graphics, Color color, Rectangle bounds, int radius)
    {
        using var path = CreateRoundedPath(bounds, radius);
        using var brush = new SolidBrush(color);

        graphics.FillPath(brush, path);
    }

    public static void DrawRoundedBorder(this Graphics graphics, Color color, Rectangle bounds, int radius, float width = 1f)
    {
        // Shrink by half the pen width so the stroke stays inside the requested bounds.
        var inner = Rectangle.Inflate(bounds, -(int)Math.Ceiling(width / 2), -(int)Math.Ceiling(width / 2));

        using var path = CreateRoundedPath(inner, radius);
        using var pen = new Pen(color, width);

        graphics.DrawPath(pen, path);
    }

    /// <summary>Enables the smoothing settings used for every custom drawn control.</summary>
    public static void UseHighQuality(this Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    }

    /// <summary>Blends <paramref name="color"/> towards <paramref name="other"/>.</summary>
    public static Color Blend(this Color color, Color other, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);

        return Color.FromArgb(
            color.A,
            (int)(color.R + ((other.R - color.R) * amount)),
            (int)(color.G + ((other.G - color.G) * amount)),
            (int)(color.B + ((other.B - color.B) * amount)));
    }
}
