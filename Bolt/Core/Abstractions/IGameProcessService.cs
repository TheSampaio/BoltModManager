namespace Bolt.Core.Abstractions;

/// <summary>Launches the game executable and tracks its lifetime.</summary>
internal interface IGameProcessService : IDisposable
{
    bool IsRunning { get; }

    event Action? GameStarted;

    event Action? GameExited;

    /// <summary>Starts <paramref name="executablePath"/> and begins watching the process.</summary>
    OperationResult Run(string executablePath);

    /// <summary>Forcefully terminates the tracked game process and its child processes.</summary>
    OperationResult Terminate();
}
