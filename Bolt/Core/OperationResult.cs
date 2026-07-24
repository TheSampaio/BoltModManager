namespace Bolt.Core;

/// <summary>
/// Outcome of an operation that can fail for expected reasons.
/// </summary>
/// <remarks>
/// Services return this instead of a bare <see cref="bool"/> so they can explain a failure
/// without depending on <c>MessageBox</c>. Deciding how to surface the message belongs to the
/// presentation layer alone.
/// </remarks>
internal readonly record struct OperationResult
{
    private OperationResult(bool succeeded, string? error, bool canceled)
    {
        Succeeded = succeeded;
        Error = error;
        WasCanceled = canceled;
    }

    public bool Succeeded { get; }

    /// <summary>True when the user deliberately aborted the operation.</summary>
    public bool WasCanceled { get; }

    public string? Error { get; }

    public bool Failed => !Succeeded;

    public static OperationResult Success() => new(true, null, false);

    public static OperationResult Failure(string error) => new(false, error, false);

    public static OperationResult Canceled(string message) => new(false, message, true);
}
