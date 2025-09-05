using Bolt.Models;
using Bolt.Utilities;

namespace Bolt.Data
{
    /// <summary>
    /// Handles loading and saving <see cref="GameModel"/> objects as JSON files.
    /// </summary>
    internal class GameData
    {
        /// <summary>
        /// Loads a <see cref="GameModel"/> from the specified file path.
        /// Returns <c>null</c> if deserialization fails.
        /// </summary>
        /// <param name="path">The file path of the saved game model.</param>
        /// <returns>The deserialized <see cref="GameModel"/> or <c>null</c>.</returns>
        public static GameModel? Load(string path) => Json.Deserialize<GameModel>(path);

        /// <summary>
        /// Saves the given <see cref="GameModel"/> to the specified file path as JSON.
        /// </summary>
        /// <param name="gameModel">The game model to save.</param>
        /// <param name="path">The file path where the game model will be saved.</param>
        public static void Save(GameModel gameModel, string path) => Json.Serialize(gameModel, path);
    }
}
