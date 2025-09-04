using System.Text.Json;

namespace Bolt.Utilities
{
    internal static class Json
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Reads a value by key from a JSON file.
        /// Returns null if the key doesn't exist.
        /// </summary>
        public static string? Read(string key, string jsonFile)
        {
            EnsureFileExists(jsonFile);

            var dict = Deserialize<Dictionary<string, string>>(jsonFile) ?? [];

            return dict.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Writes or updates a key-value pair in a JSON file.
        /// </summary>
        public static void Write(string key, string value, string jsonFile)
        {
            EnsureFileExists(jsonFile);

            var dict = Deserialize<Dictionary<string, string>>(jsonFile) ?? [];

            dict[key] = value;

            Serialize(dict, jsonFile);
        }

        /// <summary>
        /// Serializes an object and saves it to a JSON file.
        /// </summary>
        public static void Serialize<T>(T obj, string jsonFile)
        {
            EnsureFileExists(jsonFile);

            var json = JsonSerializer.Serialize(obj, SerializerOptions);
            File.WriteAllText(jsonFile, json);
        }

        /// <summary>
        /// Deserializes a JSON file into an object of type T.
        /// </summary>
        public static T? Deserialize<T>(string jsonFile)
        {
            EnsureFileExists(jsonFile);

            var content = File.ReadAllText(jsonFile);

            return JsonSerializer.Deserialize<T>(content, SerializerOptions);
        }

        /// <summary>
        /// Ensures that the file exists; creates an empty JSON object if not.
        /// </summary>
        private static void EnsureFileExists(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                File.WriteAllText(jsonFile, "{}");
        }
    }
}
