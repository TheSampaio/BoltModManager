using Bolt.Data;
using Bolt.Models;
using Bolt.Interfaces;

namespace Bolt.Services
{
    internal class GameSessionService : IGameSessionService
    {
        public event Action<GameModel>? GameLoaded;
        public event Action? GameUnloaded;

        public GameModel? CurrentGame => _currentGame;
        private GameModel? _currentGame;


        public void LoadGame(string path)
        {
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