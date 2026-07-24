using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Bolt.Infrastructure.Native;
using Bolt.Infrastructure.Storage;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>Product information and the environment Bolt is currently running in.</summary>
internal sealed class AboutForm : ThemedForm
{
    public AboutForm(string version)
    {
        Text = "About Bolt";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 322);
        Padding = new Padding(AppTheme.Spacing.XLarge, AppTheme.Spacing.XLarge, AppTheme.Spacing.XLarge, AppTheme.Spacing.Large);

        var close = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Close",
            Variant = ButtonVariant.Primary,
            Width = 100
        };

        var openDataFolder = new AppButton
        {
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, AppTheme.Spacing.Small, 0),
            Text = "Open settings folder",
            Width = 170
        };

        close.Click += (_, _) => Close();
        openDataFolder.Click += (_, _) => OpenDataFolder();

        var buttons = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 40
        };

        buttons.Controls.AddRange([openDataFolder, close]);

        var details = new Card
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, AppTheme.Spacing.Large, 0, AppTheme.Spacing.Large),
            Padding = new Padding(AppTheme.Spacing.Large, AppTheme.Spacing.Medium, AppTheme.Spacing.Large, AppTheme.Spacing.Medium)
        };

        details.Controls.Add(CreateDetail("Settings", AppPaths.DataDirectory));
        details.Controls.Add(CreateDetail("Linking", SymbolicLink.CanCreateWithoutElevation
            ? "Direct — no elevation prompt"
            : "Elevated helper — Windows asks for confirmation"));
        details.Controls.Add(CreateDetail("Version", version));

        Controls.AddRange([buttons, details, BuildHeader()]);
    }

    private static Panel BuildHeader()
    {
        var logo = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Left,
            Width = 64
        };

        logo.Paint += (_, e) => Icons.Draw(
            e.Graphics,
            IconKind.Bolt,
            new RectangleF(0, 4, 48, 48),
            AppTheme.Colors.Accent);

        var title = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Font = AppTheme.Fonts.Title,
            ForeColor = AppTheme.Colors.TextPrimary,
            Height = 32,
            Text = "Bolt Mod Manager",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var subtitle = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextSecondary,
            Height = 24,
            Text = "A mod manager that works with any game.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var text = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill };
        text.Controls.Add(subtitle);
        text.Controls.Add(title);

        var header = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 60
        };

        header.Controls.Add(text);
        header.Controls.Add(logo);

        return header;
    }

    private static Panel CreateDetail(string caption, string value)
    {
        var valueLabel = new Label
        {
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextPrimary,
            Text = value,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var captionLabel = new Label
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Left,
            Font = AppTheme.Fonts.Body,
            ForeColor = AppTheme.Colors.TextMuted,
            Text = caption,
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 80
        };

        var row = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 30
        };

        row.Controls.AddRange([valueLabel, captionLabel]);

        return row;
    }

    private static void OpenDataFolder()
    {
        try
        {
            AppPaths.EnsureDataDirectory();
            Process.Start(new ProcessStartInfo(AppPaths.DataDirectory) { UseShellExecute = true })?.Dispose();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            // Opening Explorer is a convenience; failing to do so should never interrupt the user.
        }
    }
}
