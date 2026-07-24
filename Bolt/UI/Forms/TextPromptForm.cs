using System.Drawing;
using System.Windows.Forms;
using Bolt.UI.Controls;
using Bolt.UI.Theme;

namespace Bolt.UI.Forms;

/// <summary>Small modal dialog asking the user for a single line of text.</summary>
internal sealed class TextPromptForm : ThemedForm
{
    private readonly AppTextField _field;

    public TextPromptForm(string title, string label, string initialValue = "")
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(400, 156);
        Padding = new Padding(AppTheme.Spacing.XLarge, AppTheme.Spacing.XLarge, AppTheme.Spacing.XLarge, AppTheme.Spacing.Large);

        _field = new AppTextField
        {
            Dock = DockStyle.Top,
            Text = label,
            Value = initialValue
        };

        var confirm = new AppButton
        {
            Dock = DockStyle.Right,
            Text = "Confirm",
            Variant = ButtonVariant.Primary,
            Width = 100
        };

        var cancel = new AppButton
        {
            Dock = DockStyle.Right,
            // Separates the two buttons: docked controls honour their margin.
            Margin = new Padding(0, 0, AppTheme.Spacing.Small, 0),
            Text = "Cancel",
            Width = 100
        };

        confirm.Click += (_, _) => Close(DialogResult.OK);
        cancel.Click += (_, _) => Close(DialogResult.Cancel);

        var buttons = new Panel
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 34,
            Padding = new Padding(0)
        };

        // Docked children stack in reverse order of addition, so Confirm ends up on the right.
        buttons.Controls.AddRange([cancel, confirm]);

        Controls.AddRange([buttons, _field]);
    }

    /// <summary>Text entered by the user, trimmed.</summary>
    public string Value => _field.Value.Trim();

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _field.Focus();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Enter:
                Close(DialogResult.OK);
                return true;

            case Keys.Escape:
                Close(DialogResult.Cancel);
                return true;

            default:
                return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    private void Close(DialogResult result)
    {
        DialogResult = result;
        Close();
    }
}
