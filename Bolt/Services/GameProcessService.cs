using System.ComponentModel;
using System.Diagnostics;
using Bolt.Core;
using Bolt.Core.Abstractions;

namespace Bolt.Services;

/// <summary>
/// Starts the game executable, reports when it exits, and can terminate it on request.
/// </summary>
/// <remarks>
/// Closing Bolt does not terminate the game. Termination only happens after an explicit user
/// action. The exit handler is detached and the process disposed to avoid leaking handles across
/// launches.
/// </remarks>
internal sealed class GameProcessService : IGameProcessService
{
    private readonly object _gate = new();
    private readonly Func<ProcessStartInfo, Process?> _startProcess;

    private Process? _process;

    public GameProcessService()
        : this(Process.Start)
    {
    }

    internal GameProcessService(Func<ProcessStartInfo, Process?> startProcess)
    {
        _startProcess = startProcess;
    }

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
            process = _startProcess(startInfo);
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

    public OperationResult Terminate()
    {
        var publishExit = false;

        lock (_gate)
        {
            if (_process is null)
                return OperationResult.Failure("No game process is currently running.");

            try
            {
                if (_process.HasExited)
                {
                    Detach();
                    publishExit = true;
                }
                else
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException)
            {
                return OperationResult.Failure($"The game process could not be terminated: {ex.Message}");
            }
        }

        if (publishExit)
            GameExited?.Invoke();

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
