using System.Diagnostics;

namespace Bolt.Services
{
    internal class GameProcessService : IDisposable
    {
        public static GameProcessService Instance => _instance.Value;
        private static readonly Lazy<GameProcessService> _instance = new(() => new GameProcessService());

        public bool IsRunning => _process is not null && !_process.HasExited;

        public event Action? GameStarted;
        public event Action? GameExited;

        private Process? _process;

        /// <summary>
        /// Starts the game process using the executable path.
        /// </summary>
        public void RunGame(string executablePath)
        {
            if (IsRunning)
            {
                MessageBox.Show(
                    "A game is already running. Please close it before starting another instance.",
                    "Game Already Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = true
            };

            _process = Process.Start(startInfo);

            if (_process is null)
            {
                MessageBox.Show(
                    "Failed to start the game process. Please check the executable path.",
                    "Error Starting Game",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            _process.EnableRaisingEvents = true;
            _process.Exited += (s, e) =>
            {
                _process = null;
                GameExited?.Invoke();
            };

            GameStarted?.Invoke();
        }

        /// <summary>
        /// Attempts to close the running game process.
        /// </summary>
        public void CloseGame()
        {
            if (_process is null || _process.HasExited)
                return;

            try
            {
                _process.Kill();
                _process.WaitForExit();
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                   $"Failed to close the game process.\n\nError: {ex.Message}",
                   "Error Closing Game",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error
               );
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
