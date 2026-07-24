namespace Bolt.Infrastructure.Storage;

/// <summary>Path helpers shared by the storage and deployment code.</summary>
internal static class PathUtility
{
    private static readonly char[] InvalidNameCharacters =
        [.. Path.GetInvalidFileNameChars(), .. new[] { '/', '\\' }];

    /// <summary>
    /// Turns arbitrary text into a name usable as a single folder name.
    /// </summary>
    public static string ToSafeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Unnamed";

        var sanitized = new string([.. name.Select(c => InvalidNameCharacters.Contains(c) ? '_' : c)]).Trim(' ', '.');

        return sanitized.Length == 0 ? "Unnamed" : sanitized;
    }

    /// <summary>
    /// True when <paramref name="candidate"/> is inside <paramref name="root"/>.
    /// </summary>
    /// <remarks>
    /// Compared segment-wise on full paths so that <c>C:\Mods2</c> is not treated as being inside
    /// <c>C:\Mods</c>. This is what protects the extraction code against zip-slip entries.
    /// </remarks>
    public static bool IsInside(string root, string candidate)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var normalizedCandidate = Path.GetFullPath(candidate);

        return normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Normalizes a relative path to backslash separators without a leading separator.</summary>
    public static string NormalizeRelative(string relativePath) =>
        relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

    /// <summary>Removes <paramref name="directory"/> and every empty parent up to (but excluding) <paramref name="stopAt"/>.</summary>
    public static void DeleteEmptyDirectories(string directory, string stopAt)
    {
        var current = directory;

        while (!string.IsNullOrEmpty(current)
            && IsInside(stopAt, current)
            && !Path.GetFullPath(current).Equals(Path.GetFullPath(stopAt), StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(current)
            && !Directory.EnumerateFileSystemEntries(current).Any())
        {
            Directory.Delete(current);
            current = Path.GetDirectoryName(current);
        }
    }
}
