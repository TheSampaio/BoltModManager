using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>One requested change to a file owned by a modification.</summary>
/// <param name="SourcePath">Current path relative to the modification folder.</param>
/// <param name="DestinationPath">New path relative to both the modification and game folders.</param>
internal readonly record struct ModFileEdit(string SourcePath, string DestinationPath);

/// <summary>Editable metadata and file layout of a modification.</summary>
internal sealed record ModificationEdit(
    string Name,
    string Description,
    string Version,
    string Category,
    IReadOnlyList<ModFileEdit> Files);

/// <summary>Updates an imported modification without rebuilding its source archive.</summary>
internal interface IModEditorService
{
    /// <summary>
    /// Applies metadata and relative file path changes, keeping deployed links synchronized.
    /// </summary>
    OperationResult Apply(GameSession session, Modification modification, ModificationEdit edit);
}
