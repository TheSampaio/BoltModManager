using Newtonsoft.Json.Linq;

namespace Bolt.Utilities
{
    internal class Json
    {
        private static void EnsureFileExists(string jsonFile)
        {
            // Create an empty JSON file if it doesn't exist
            if (!File.Exists(jsonFile))
                File.WriteAllText(jsonFile, "{}");
        }

        public static string? Read(string key, string jsonFile)
        {
            try
            {
                // Ensure the file exists before reading
                EnsureFileExists(jsonFile);

                // Read the JSON content from the file
                string content = File.ReadAllText(jsonFile);

                // Parse the JSON content into a JObject
                JObject jsonObject = JObject.Parse(content);

                // Get the value associated with the specified key
                JToken? value = jsonObject[key];

                // Return the value as a string, or null if the key doesn't exist
                return value?.ToString();
            }

            catch (Exception ex)
            {
                // Display error message if something goes wrong
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

        public static bool Write(string key, string value, string jsonFile)
        {
            try
            {
                // Ensure the file exists before writing
                EnsureFileExists(jsonFile);

                // Read the existing JSON content
                string content = File.ReadAllText(jsonFile);

                // Parse the JSON content into a JObject
                JObject jsonObject = JObject.Parse(content);

                // Update or add the key-value pair
                jsonObject[key] = value;

                // Save the updated JSON back to the file
                File.WriteAllText(jsonFile, jsonObject.ToString());

                return true;
            }

            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }
    }
}
