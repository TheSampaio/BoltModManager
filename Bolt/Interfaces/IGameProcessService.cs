namespace Bolt.Interfaces
{
    public interface IGameProcessService : IDisposable
    {
        bool IsRunning { get; }
        event Action? GameStarted;
        event Action? GameExited;

        void RunGame(string executablePath);
        void CloseGame();
    }
}