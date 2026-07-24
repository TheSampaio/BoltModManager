using System.Windows.Forms;
using Bolt.Core.Abstractions;

namespace Bolt.UI.Services;

/// <summary>
/// Shows messages to the user. The only place in the application allowed to call
/// <see cref="MessageBox"/>, which keeps the services free of any presentation concern.
/// </summary>
internal sealed class DialogService : IDialogService
{
    public void Info(string message, string caption = "Bolt") =>
        Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void Warning(string message, string caption = "Bolt") =>
        Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public void Error(string message, string caption = "Bolt") =>
        Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public bool Confirm(string message, string caption = "Bolt", bool destructive = false) =>
        Show(
            message,
            caption,
            MessageBoxButtons.YesNo,
            destructive ? MessageBoxIcon.Warning : MessageBoxIcon.Question,
            destructive ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1) == DialogResult.Yes;

    private static DialogResult Show(
        string message,
        string caption,
        MessageBoxButtons buttons,
        MessageBoxIcon icon,
        MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
    {
        var owner = Form.ActiveForm;

        return owner is null
            ? MessageBox.Show(message, caption, buttons, icon, defaultButton)
            : MessageBox.Show(owner, message, caption, buttons, icon, defaultButton);
    }
}
