using Bolt.Data;
using Bolt.Models;
using Bolt.Interfaces;
using Bolt.Utilities;
using System.IO.Compression;

namespace Bolt.Services
{
    internal class ModImportService : IModImportService
    {
        public async Task ImportModsAsync(string[] zipFiles, GameModel currentGame, ProfileModel currentProfile, IProgress<int> progress)
        {
            var modificationsPath = currentGame.ModificationsPath;

            foreach (var selectedFile in zipFiles)
            {
                if (Path.GetExtension(selectedFile)?.ToLower() != ".zip")
                    continue;

                string modificationName = Path.GetFileNameWithoutExtension(selectedFile);
                string destinationPath = Path.Combine(modificationsPath, modificationName);

                if (System.IO.Directory.Exists(destinationPath))
                    System.IO.Directory.Delete(destinationPath, true);

                System.IO.Directory.CreateDirectory(destinationPath);

                using var archive = Archive.OpenRead(selectedFile);

                int current = 0;

                string topLevelFolder = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.FullName) && e.FullName.Contains('/'))
                    .Select(e => e.FullName.Split('/')[0])
                    .FirstOrDefault() ?? string.Empty;

                List<string> modificationContent = [];

                foreach (var entry in archive.Entries)
                {
                    string relativePath = entry.FullName;

                    if (!string.IsNullOrEmpty(topLevelFolder) && relativePath.StartsWith(topLevelFolder))
                        relativePath = relativePath[topLevelFolder.Length..].TrimStart('/', '\\');

                    relativePath = relativePath
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim()
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);

                    foreach (char invalidChar in Path.GetInvalidPathChars())
                        relativePath = relativePath.Replace(invalidChar, '_');

                    string destinationFile = Path.Combine(destinationPath, relativePath);

                    string directory = Path.GetDirectoryName(destinationFile)!;
                    string fileName = Path.GetFileName(destinationFile);

                    foreach (char invalidChar in Path.GetInvalidFileNameChars())
                        fileName = fileName.Replace(invalidChar, '_');

                    destinationFile = Path.Combine(directory, fileName);

                    if (!string.IsNullOrEmpty(relativePath))
                        modificationContent.Add(destinationFile);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        System.IO.Directory.CreateDirectory(destinationFile);
                        continue;
                    }

                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                    await Task.Run(() => entry.ExtractToFile(destinationFile, true));

                    current++;
                    progress.Report(current);
                }

                var currentModification = new ModificationModel()
                {
                    Id = Guid.NewGuid(),
                    Name = modificationName,
                    Version = "N/A",
                    Category = "N/A",
                    InstalledAt = DateTime.UtcNow,
                    Content = modificationContent
                };

                currentProfile.Modifications.Add(currentModification);
                string gameFilename = $"{AppData.GamesPath}\\{currentGame.Name}\\{AppData.GameFile}";
                GameData.Save(currentGame, gameFilename);
            }
        }
    }
}