using Bolt.Data;
using Bolt.Models;

namespace Bolt.Services
{
    internal class GameDataService : IDisposable
    {
        public event Action<GameModel>? GameLoaded;
        public event Action? GameUnloaded;
        public GameModel? CurrentGame => _currentGame;
        public static GameDataService Instance => _instance.Value;

        private GameModel? _currentGame;
        private static readonly Lazy<GameDataService> _instance = new(() => new GameDataService());

        public void LoadGame(string path)
        {
            // Forces to unload the current game before load another
            if (_currentGame is not null)
                UnloadGame();

            _currentGame = GameData.Load(path);

            if (_currentGame is not null)
                GameLoaded?.Invoke(_currentGame);
        }

        public void UnloadGame()
        {
            _currentGame = null;
            GameUnloaded?.Invoke();
        }

        public void Dispose()
        {
            _currentGame = null;
            GameLoaded = null;
            GameUnloaded = null;
        }
    }
}
