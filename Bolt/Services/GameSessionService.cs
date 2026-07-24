using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;

namespace Bolt.Services;

/// <summary>
/// Keeps track of the game currently open and of the file it came from.
/// </summary>
internal sealed class GameSessionService(IGameRepository repository, IUserPreferencesService preferences)
    : IGameSessionService
{
    private readonly IGameRepository _repository = repository;
    private readonly IUserPreferencesService _preferences = preferences;

    public event Action<GameSession>? GameLoaded;
    public event Action? GameUnloaded;
    public event Action<GameSession>? GameChanged;

    public GameSession? Current { get; private set; }

    public OperationResult Load(string path)
    {
        GameSession session;

        try
        {
            session = _repository.Load(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A game that cannot be opened should not stay in the recent list forever.
            _preferences.RemoveRecentGame(path);
            return OperationResult.Failure(ex.Message);
        }

        Unload();

        Current = session;
        _preferences.AddRecentGame(session.FilePath);

        GameLoaded?.Invoke(session);

        return OperationResult.Success();
    }

    public void Unload()
    {
        if (Current is null)
            return;

        Current = null;
        GameUnloaded?.Invoke();
    }

    public OperationResult Save()
    {
        if (Current is null)
            return OperationResult.Failure("There is no game to save.");

        try
        {
            _repository.Save(Current);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return OperationResult.Failure($"The game could not be saved: {ex.Message}");
        }

        GameChanged?.Invoke(Current);

        return OperationResult.Success();
    }
}
