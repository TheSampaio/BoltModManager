using Bolt.Models;
using Bolt.Utilities;

namespace Bolt.Data
{
    internal class GameData
    {
        public static GameModel? Load(string path) => Json.Deserialize<GameModel>(path);

        public static void Save(GameModel gameModel, string path) => Json.Serialize(gameModel, path);
    }
}
