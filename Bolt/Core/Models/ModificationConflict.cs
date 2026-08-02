namespace Bolt.Core.Models;

/// <summary>A set of game paths claimed by the same pair of enabled modifications.</summary>
internal sealed record ModificationConflict(
    Guid LeftModificationId,
    string LeftModificationName,
    Guid RightModificationId,
    string RightModificationName,
    IReadOnlyList<string> Files);
