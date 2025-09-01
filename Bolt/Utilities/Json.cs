using Newtonsoft.Json.Linq;

namespace Bolt.Utilities
{
    internal class Json
    {
        public static string? Read(string key, string jsonFile)
        {
            try
            {
                // Parse the Json content into a JObject
                var jsonObject = ParseContent(jsonFile);

                // Get the value associated with the specified key
                JToken? value = jsonObject[key];

                // Return the value as a string, or null if the key doesn't exist
                return value?.ToString();
            }

            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return null;
            }
        }

        public static bool Write(string key, string value, string jsonFile)
        {
            try
            {
                // Parse the Json content into a JObject
                var jsonObject = ParseContent(jsonFile);

                // Update or add the key-value pair
                jsonObject[key] = value;

                // Save the updated JSON back to the file
                File.WriteAllText(jsonFile, jsonObject.ToString());

                return true;
            }

            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

        public static bool SerializeAndSave(object obj, string jsonFile)
        {
            try
            {
                // Cria o JObject a partir do objeto
                JObject jsonObject = JObject.FromObject(obj);

                // Escreve no arquivo (formatado)
                File.WriteAllText(jsonFile, jsonObject.ToString());

                return true;
            }

            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

        private static void EnsureFileExists(string jsonFile)
        {
            // Create an empty JSON file if it doesn't exist
            if (!File.Exists(jsonFile))
                File.WriteAllText(jsonFile, "{}");
        }

        private static JObject ParseContent(string jsonFile)
        {
            // Ensure the file exists before writing
            EnsureFileExists(jsonFile);

            // Read the existing JSON content
            string content = File.ReadAllText(jsonFile);

            // Parse the JSON content into a JObject
            return JObject.Parse(content);
        }
    }
}
