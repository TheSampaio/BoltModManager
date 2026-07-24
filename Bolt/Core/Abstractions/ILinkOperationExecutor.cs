using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>Applies a batch of <see cref="LinkOperation"/> to the file system.</summary>
internal interface ILinkOperationExecutor
{
    /// <summary>
    /// Executes every operation, elevating only when the current process is not allowed to
    /// perform them. An empty batch succeeds without touching the file system.
    /// </summary>
    OperationResult Apply(IReadOnlyList<LinkOperation> operations);
}
