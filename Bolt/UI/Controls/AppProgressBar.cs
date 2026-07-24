using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Controls;

/// <summary>Slim rounded progress indicator.</summary>
internal sealed class AppProgressBar : Control
{
    private const int BandWidthPercent = 30;

    private readonly System.Windows.Forms.Timer _animation;

    private int _maximum = 100;
    private int _value;
    private bool _isIndeterminate;
    private int _bandOffset;

    public AppProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

        BackColor = Color.Transparent;
        Height = 6;

        _animation = new System.Windows.Forms.Timer { Interval = 30 };
        _animation.Tick += (_, _) =>
        {
            _bandOffset = (_bandOffset + 3) % (Width + (Width * BandWidthPercent / 100));
            Invalidate();
        };
    }

    /// <summary>
    /// Shows a sliding band instead of a filled portion, for the phases where the total amount of
    /// work is not known yet.
    /// </summary>
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set
        {
            if (_isIndeterminate == value)
                return;

            _isIndeterminate = value;
            _bandOffset = 0;
            _animation.Enabled = value;

            Invalidate();
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(1, value);
            _value = Math.Min(_value, _maximum);
            Invalidate();
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, 0, _maximum);

            if (clamped == _value)
                return;

            _value = clamped;
            Invalidate();
        }
    }

    protected override Size DefaultSize => new(200, 6);

    public void Reset()
    {
        _value = 0;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.UseHighQuality();

        var radius = Height / 2;
        var track = new Rectangle(0, 0, Width - 1, Height - 1);

        graphics.FillRoundedRectangle(AppTheme.Colors.SurfaceActive, track, radius);

        if (_isIndeterminate)
        {
            PaintBand(graphics, track, radius);
            return;
        }

        if (_value <= 0)
            return;

        var filledWidth = (int)(track.Width * ((double)_value / _maximum));

        if (filledWidth < Height)
            filledWidth = Height;

        graphics.FillRoundedRectangle(AppTheme.Colors.AccentText, track with { Width = filledWidth }, radius);
    }

    private void PaintBand(Graphics graphics, Rectangle track, int radius)
    {
        var bandWidth = Math.Max(track.Width * BandWidthPercent / 100, Height * 3);
        var left = _bandOffset - bandWidth;

        var band = new Rectangle(
            Math.Max(left, 0),
            track.Y,
            Math.Min(bandWidth + Math.Min(left, 0), track.Width - Math.Max(left, 0)),
            track.Height);

        if (band.Width > 0)
            graphics.FillRoundedRectangle(AppTheme.Colors.AccentText, band, radius);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _animation.Dispose();

        base.Dispose(disposing);
    }
}
