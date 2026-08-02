namespace Bolt.Core.Models;

/// <summary>
/// Mutable working copy of profile precedence used by the conflict manager. Later modifications
/// win shared destination paths during deployment.
/// </summary>
internal sealed class ModificationConflictOrder
{
    private readonly List<Guid> _modificationIds;

    public ModificationConflictOrder(IEnumerable<Guid> modificationIds)
    {
        ArgumentNullException.ThrowIfNull(modificationIds);

        _modificationIds = modificationIds.ToList();

        if (_modificationIds.Count != _modificationIds.Distinct().Count())
            throw new ArgumentException("Modification order cannot contain duplicate identifiers.", nameof(modificationIds));
    }

    public IReadOnlyList<Guid> ModificationIds => _modificationIds;

    public ConflictPosition GetPosition(Guid modificationId, Guid relativeToId)
    {
        var modificationIndex = FindIndex(modificationId);
        var relativeIndex = FindIndex(relativeToId);

        if (modificationIndex == relativeIndex)
            throw new ArgumentException("A modification cannot be ordered relative to itself.");

        return modificationIndex < relativeIndex
            ? ConflictPosition.Before
            : ConflictPosition.After;
    }

    public void SetPosition(Guid modificationId, Guid relativeToId, ConflictPosition position)
    {
        if (modificationId == relativeToId)
            throw new ArgumentException("A modification cannot be ordered relative to itself.");

        FindIndex(modificationId);
        FindIndex(relativeToId);

        _modificationIds.Remove(modificationId);
        var relativeIndex = FindIndex(relativeToId);
        var insertionIndex = position == ConflictPosition.Before
            ? relativeIndex
            : relativeIndex + 1;

        _modificationIds.Insert(insertionIndex, modificationId);
    }

    private int FindIndex(Guid modificationId)
    {
        var index = _modificationIds.IndexOf(modificationId);

        return index >= 0
            ? index
            : throw new ArgumentException($"Modification \"{modificationId}\" is not part of this order.");
    }
}
