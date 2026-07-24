using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bolt.UI.Theme;

/// <summary>
/// Vector icon set drawn with GDI+.
/// </summary>
/// <remarks>
/// Icons are geometry rather than bitmaps, so they stay sharp at every DPI and always match the
/// active palette. This also removes the bitmap resources the old interface depended on.
/// </remarks>
internal static class Icons
{
    /// <summary>Side of the coordinate system every glyph is designed in.</summary>
    private const float DesignSize = 24f;

    /// <summary>Draws <paramref name="kind"/> centred inside <paramref name="bounds"/>.</summary>
    /// <param name="strokeWidth">Line width expressed in design units.</param>
    public static void Draw(Graphics graphics, IconKind kind, RectangleF bounds, Color color, float strokeWidth = 2f)
    {
        if (kind == IconKind.None || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var state = graphics.Save();

        try
        {
            graphics.UseHighQuality();

            var scale = Math.Min(bounds.Width, bounds.Height) / DesignSize;

            graphics.TranslateTransform(
                bounds.X + ((bounds.Width - (DesignSize * scale)) / 2f),
                bounds.Y + ((bounds.Height - (DesignSize * scale)) / 2f));

            graphics.ScaleTransform(scale, scale);

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using var brush = new SolidBrush(color);

            Render(graphics, kind, pen, brush);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private static void Render(Graphics g, IconKind kind, Pen pen, Brush brush)
    {
        switch (kind)
        {
            case IconKind.Bolt:
                g.FillPolygon(brush, [
                    new PointF(13.5f, 2f), new PointF(4.5f, 13.5f), new PointF(10.5f, 13.5f),
                    new PointF(9.5f, 22f), new PointF(19.5f, 10f), new PointF(13.5f, 10f)]);
                break;

            case IconKind.Play:
                g.FillPolygon(brush, [new PointF(7f, 4.5f), new PointF(19.5f, 12f), new PointF(7f, 19.5f)]);
                break;

            case IconKind.Plus:
                g.DrawLine(pen, 12f, 5f, 12f, 19f);
                g.DrawLine(pen, 5f, 12f, 19f, 12f);
                break;

            case IconKind.Trash:
                g.DrawLine(pen, 3.5f, 6.5f, 20.5f, 6.5f);
                g.DrawLines(pen, [new PointF(9f, 6.5f), new PointF(9f, 3.5f), new PointF(15f, 3.5f), new PointF(15f, 6.5f)]);
                g.DrawLines(pen, [new PointF(5.5f, 6.5f), new PointF(6.5f, 20.5f), new PointF(17.5f, 20.5f), new PointF(18.5f, 6.5f)]);
                g.DrawLine(pen, 10f, 10.5f, 10f, 16.5f);
                g.DrawLine(pen, 14f, 10.5f, 14f, 16.5f);
                break;

            case IconKind.Download:
                g.DrawLine(pen, 12f, 3.5f, 12f, 15f);
                g.DrawLines(pen, [new PointF(7.5f, 10.5f), new PointF(12f, 15f), new PointF(16.5f, 10.5f)]);
                g.DrawLines(pen, [new PointF(4f, 16f), new PointF(4f, 20.5f), new PointF(20f, 20.5f), new PointF(20f, 16f)]);
                break;

            case IconKind.Folder:
                g.DrawLines(pen, [
                    new PointF(3f, 19f), new PointF(3f, 5.5f), new PointF(9.5f, 5.5f), new PointF(11.5f, 8.5f),
                    new PointF(21f, 8.5f), new PointF(21f, 19f), new PointF(3f, 19f)]);
                break;

            case IconKind.Ellipsis:
                foreach (var x in new[] { 6f, 12f, 18f })
                    g.FillEllipse(brush, x - 1.5f, 10.5f, 3f, 3f);
                break;

            case IconKind.Sliders:
                g.DrawLine(pen, 3.5f, 7f, 20.5f, 7f);
                g.DrawLine(pen, 3.5f, 17f, 20.5f, 17f);
                DrawKnob(g, pen, 9f, 7f);
                DrawKnob(g, pen, 15f, 17f);
                break;

            case IconKind.Search:
                g.DrawEllipse(pen, 4f, 4f, 12.5f, 12.5f);
                g.DrawLine(pen, 15.5f, 15.5f, 20f, 20f);
                break;

            case IconKind.Check:
                g.DrawLines(pen, [new PointF(5f, 12.5f), new PointF(10f, 17.5f), new PointF(19f, 6.5f)]);
                break;

            case IconKind.Ban:
                g.DrawEllipse(pen, 3.5f, 3.5f, 17f, 17f);
                g.DrawLine(pen, 6.5f, 6.5f, 17.5f, 17.5f);
                break;

            case IconKind.Refresh:
                g.DrawArc(pen, 4f, 4f, 16f, 16f, 70f, 240f);
                g.DrawLines(pen, [new PointF(16f, 2.5f), new PointF(17.5f, 7.2f), new PointF(12.8f, 8.2f)]);
                break;

            case IconKind.ChevronDown:
                g.DrawLines(pen, [new PointF(6.5f, 9.5f), new PointF(12f, 15f), new PointF(17.5f, 9.5f)]);
                break;

            case IconKind.Info:
                g.DrawEllipse(pen, 3.5f, 3.5f, 17f, 17f);
                g.DrawLine(pen, 12f, 11f, 12f, 16.5f);
                g.FillEllipse(brush, 10.9f, 6.9f, 2.2f, 2.2f);
                break;

            case IconKind.Close:
                g.DrawLine(pen, 6.5f, 6.5f, 17.5f, 17.5f);
                g.DrawLine(pen, 17.5f, 6.5f, 6.5f, 17.5f);
                break;

            case IconKind.Package:
                g.DrawLines(pen, [
                    new PointF(12f, 2.5f), new PointF(20.5f, 7.25f), new PointF(20.5f, 16.75f),
                    new PointF(12f, 21.5f), new PointF(3.5f, 16.75f), new PointF(3.5f, 7.25f),
                    new PointF(12f, 2.5f)]);
                g.DrawLines(pen, [new PointF(3.5f, 7.25f), new PointF(12f, 12f), new PointF(20.5f, 7.25f)]);
                g.DrawLine(pen, 12f, 12f, 12f, 21.5f);
                break;

            case IconKind.Warning:
                g.DrawLines(pen, [
                    new PointF(12f, 3.5f), new PointF(21.5f, 20f), new PointF(2.5f, 20f), new PointF(12f, 3.5f)]);
                g.DrawLine(pen, 12f, 9.5f, 12f, 14f);
                g.FillEllipse(brush, 10.9f, 16.4f, 2.2f, 2.2f);
                break;
        }
    }

    private static void DrawKnob(Graphics g, Pen pen, float x, float y)
    {
        const float radius = 2.6f;

        g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
    }
}
