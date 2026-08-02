namespace Bolt.Core.Models;

/// <summary>Relative deployment position of one modification compared with another.</summary>
internal enum ConflictPosition
{
    /// <summary>The modification is deployed first and loses shared files.</summary>
    Before,

    /// <summary>The modification is deployed last and wins shared files.</summary>
    After
}
