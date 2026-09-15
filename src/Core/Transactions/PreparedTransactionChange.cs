namespace Plaquewright.Core.Transactions;

/// <summary>
/// Represents a change that has already passed all normal
/// transaction validation.
///
/// ApplyCore must not perform new gameplay validation and must not
/// intentionally invoke operations that are expected to fail.
///
/// A prepared change can be applied exactly once.
/// </summary>
public abstract class PreparedTransactionChange
{
    private bool _wasApplied;

    protected PreparedTransactionChange()
    {
    }

    internal void Apply()
    {
        if (_wasApplied)
        {
            throw new InvalidOperationException(
                "A prepared transaction change cannot be applied more than once.");
        }

        _wasApplied = true;

        ApplyCore();
    }

    protected abstract void ApplyCore();
}