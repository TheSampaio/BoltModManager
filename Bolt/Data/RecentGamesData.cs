using Bolt.Utilities;

namespace Bolt.Data
{
    /// <summary>
    /// Handles loading, saving, and clearing the recent games history in user preferences.
    /// </summary>
    internal class RecentGamesData
    {
        private const string _KEY = "RecentGames";
        private const int MaxRecentGames = 10;

        public static List<string> Load()
        {
            var data = Json.Read(_KEY, AppData.PreferencesFile);
            if (string.IsNullOrEmpty(data)) return [];

            return [.. data.Split('|', StringSplitOptions.RemoveEmptyEntries)];
        }

        public static void Save(string gamePath)
        {
            var list = Load();

            list.Remove(gamePath);
            list.Insert(0, gamePath);

            if (list.Count > MaxRecentGames)
                list = list.Take(MaxRecentGames).ToList();

            Json.Write(_KEY, string.Join('|', list), AppData.PreferencesFile);
        }

        public static void Clear()
        {
            Json.Write(_KEY, string.Empty, AppData.PreferencesFile);
        }
    }
}