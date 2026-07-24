namespace Bolt.Core.Abstractions;

/// <summary>
/// Abstracts every message shown to the user, keeping <c>MessageBox</c> out of the services.
/// </summary>
internal interface IDialogService
{
    void Info(string message, string caption = "Bolt");

    void Warning(string message, string caption = "Bolt");

    void Error(string message, string caption = "Bolt");

    /// <summary>Asks a yes/no question. Returns true when the user confirms.</summary>
    bool Confirm(string message, string caption = "Bolt", bool destructive = false);
}
