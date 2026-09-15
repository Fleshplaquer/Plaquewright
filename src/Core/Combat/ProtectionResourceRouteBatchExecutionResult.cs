namespace Plaquewright.Core.Combat;

public sealed class ProtectionResourceRouteBatchExecutionResult
{
    private readonly ProtectionResourceRouteBatchItemResult[]
        _items;

    public ProtectionAssignmentRoutingState PreviousState { get; }

    public ProtectionAssignmentRoutingState UpdatedState { get; }

    public IReadOnlyList<ProtectionResourceRouteBatchItemResult>
        Items
    { get; }

    internal ProtectionResourceRouteBatchExecutionResult(
        ProtectionAssignmentRoutingState previousState,
        ProtectionAssignmentRoutingState updatedState,
        ProtectionResourceRouteBatchItemResult[] items)
    {
        ArgumentNullException.ThrowIfNull(
            previousState);

        ArgumentNullException.ThrowIfNull(
            updatedState);

        ArgumentNullException.ThrowIfNull(
            items);

        PreviousState =
            previousState;

        UpdatedState =
            updatedState;

        _items =
            items;

        Items =
            Array.AsReadOnly(
                _items);
    }
}