using Bolt.Utilities;

namespace Bolt.Data
{
    /// <summary>
    /// Handles reading and writing the path to the packages directory in user preferences.
    /// </summary>
    internal class ModificationsData
    {
        /// <summary>
        /// The key used to store the packages directory in the preferences file.
        /// </summary>
        private const string _KEY = "PackagesDirectory";

        /// <summary>
        /// Loads the saved packages directory path from the preferences file.
        /// Returns <c>null</c> if no value is saved.
        /// </summary>
        public static string? Load() => Json.Read(_KEY, AppData.PreferencesFile);

        /// <summary>
        /// Saves the specified packages directory path to the preferences file.
        /// </summary>
        /// <param name="value">The path to save.</param>
        public static void Save(string value) => Json.Write(_KEY, value, AppData.PreferencesFile);
    }
}
