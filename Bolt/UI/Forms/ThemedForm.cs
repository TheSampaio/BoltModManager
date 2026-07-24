using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>
/// Base window that applies the palette, themes the title bar and gives derived forms a single
/// place to attach and detach their event handlers.
/// </summary>
/// <remarks>
/// Subscriptions are torn down exactly once, and only when they were actually created — the
/// previous base class unsubscribed on dispose even for windows that were never shown.
/// </remarks>
internal class ThemedForm : Form
{
    private static readonly Icon? ApplicationIcon = LoadApplicationIcon();

    private bool _eventsInitialized;

    protected ThemedForm()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.Background;
        ForeColor = AppTheme.Colors.TextPrimary;
        Font = AppTheme.Fonts.Body;
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        Icon = ApplicationIcon;
    }

    /// <summary>
    /// Uses the icon embedded in the executable so every window shares the product identity
    /// without duplicating the file as an embedded resource.
    /// </summary>
    private static Icon? LoadApplicationIcon()
    {
        try
        {
            return Environment.ProcessPath is { Length: > 0 } path
                ? Icon.ExtractAssociatedIcon(path)
                : null;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowTheme.Apply(this);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_eventsInitialized)
            return;

        InitializeEvents();
        _eventsInitialized = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _eventsInitialized)
        {
            TerminateEvents();
            _eventsInitialized = false;
        }

        base.Dispose(disposing);
    }

    /// <summary>Attaches the event handlers of the window. Called once, on load.</summary>
    protected virtual void InitializeEvents()
    {
    }

    /// <summary>Detaches everything attached by <see cref="InitializeEvents"/>.</summary>
    protected virtual void TerminateEvents()
    {
    }
}
