using System.Diagnostics;
using Bolt.Interfaces;

namespace Bolt.Services
{
    internal class GameProcessService : IGameProcessService
    {
        public bool IsRunning => _process is not null && !_process.HasExited;

        public event Action? GameStarted;
        public event Action? GameExited;
        private Process? _process;

        public void RunGame(string executablePath)
        {
            if (IsRunning)
                throw new InvalidOperationException("A game is already running. Please close it before starting another instance.");

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = true
            };

            _process = Process.Start(startInfo);

            if (_process is null)
                throw new InvalidOperationException("Failed to start the game process. Please check the executable path.");

            _process.EnableRaisingEvents = true;
            _process.Exited += (s, e) =>
            {
                _process = null;
                GameExited?.Invoke();
            };

            GameStarted?.Invoke();
        }

        public void CloseGame()
        {
            if (_process is null || _process.HasExited)
                return;

            try
            {
                _process.Kill();
                _process.WaitForExit();
            }
            finally
            {
                _process = null;
                GameExited?.Invoke();
            }
        }

        public void Dispose()
        {
            CloseGame();
            GameStarted = null;
            GameExited = null;
        }
    }
}