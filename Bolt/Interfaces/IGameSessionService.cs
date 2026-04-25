using Bolt.Models;

namespace Bolt.Interfaces
{
    internal interface IGameSessionService : IDisposable
    {
        event Action<GameModel>? GameLoaded;
        event Action? GameUnloaded;
        GameModel? CurrentGame { get; }

        void LoadGame(string path);
        void UnloadGame();
    }
}