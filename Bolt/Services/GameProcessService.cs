using System.ComponentModel;
using System.Diagnostics;
using Bolt.Core;
using Bolt.Core.Abstractions;

namespace Bolt.Services;

/// <summary>
/// Starts the game executable and reports when it exits.
/// </summary>
/// <remarks>
/// The process is only observed, never terminated: closing Bolt must not close a game the user is
/// still playing. The exit handler is detached and the process disposed to avoid leaking handles
/// across launches.
/// </remarks>
internal sealed class GameProcessService : IGameProcessService
{
    private readonly object _gate = new();

    private Process? _process;

    public event Action? GameStarted;
    public event Action? GameExited;

    public bool IsRunning
    {
        get
        {
            lock (_gate)
                return _process is { HasExited: false };
        }
    }

    public OperationResult Run(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            return OperationResult.Failure("This game has no executable configured.");

        if (!File.Exists(executablePath))
            return OperationResult.Failure($"The executable \"{executablePath}\" was not found.");

        if (IsRunning)
            return OperationResult.Failure("The game is already running.");

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath),
            UseShellExecute = true
        };

        Process? process;

        try
        {
            process = Process.Start(startInfo);
        }
        catch (Win32Exception ex)
        {
            return OperationResult.Failure($"The game could not be started: {ex.Message}");
        }

        if (process is null)
            return OperationResult.Failure("The game could not be started.");

        lock (_gate)
        {
            _process = process;
            process.Exited += OnProcessExited;
        }

        GameStarted?.Invoke();

        // Start observing only after subscribers know that the game started. Otherwise a process
        // which exits immediately can publish GameExited before GameStarted and leave the UI locked.
        lock (_gate)
        {
            if (ReferenceEquals(process, _process))
                process.EnableRaisingEvents = true;
        }

        return OperationResult.Success();
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        lock (_gate)
        {
            if (sender is not Process process || !ReferenceEquals(process, _process))
                return;

            Detach();
        }

        GameExited?.Invoke();
    }

    /// <summary>Releases the tracked process. Must be called while holding <see cref="_gate"/>.</summary>
    private void Detach()
    {
        if (_process is null)
            return;

        _process.Exited -= OnProcessExited;
        _process.Dispose();
        _process = null;
    }

    public void Dispose()
    {
        lock (_gate)
            Detach();

        GameStarted = null;
        GameExited = null;
    }
}
