using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bolt.Infrastructure.Storage;

/// <summary>
/// Reads and writes JSON documents on disk.
/// </summary>
/// <remarks>
/// Unlike the previous helper, reading a missing file never creates it and never fabricates an
/// empty object: callers get <c>null</c> and can tell "absent" from "present but empty" apart.
/// Writing is atomic (temporary file plus replace) so a crash cannot truncate a game file.
/// </remarks>
internal static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Deserializes <paramref name="path"/>, or returns <c>null</c> when unavailable.</summary>
    /// <exception cref="JsonException">The file exists but does not contain valid JSON.</exception>
    public static T? Read<T>(string path) where T : class
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        var content = File.ReadAllText(path);

        return string.IsNullOrWhiteSpace(content)
            ? null
            : JsonSerializer.Deserialize<T>(content, Options);
    }

    /// <summary>Deserializes <paramref name="path"/>, falling back to <paramref name="fallback"/>.</summary>
    public static T ReadOrDefault<T>(string path, Func<T> fallback) where T : class
    {
        try
        {
            return Read<T>(path) ?? fallback();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return fallback();
        }
    }

    /// <summary>Serializes <paramref name="value"/> to <paramref name="path"/> atomically.</summary>
    public static void Write<T>(T value, string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(value, Options);
        var temporaryPath = path + ".tmp";

        File.WriteAllText(temporaryPath, json);

        // File.Move with overwrite is atomic on NTFS and avoids leaving a truncated target behind.
        File.Move(temporaryPath, path, overwrite: true);
    }
}
