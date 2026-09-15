namespace Plaquewright.Core.Combat;

public sealed class HitScopedProtectionBudgetRouteBatchExecutionResult
{
    private readonly HitScopedProtectionBudgetRouteBatchItemResult[]
        _items;

    public ProtectionAssignmentRoutingState PreviousState { get; }

    public ProtectionAssignmentRoutingState UpdatedState { get; }

    public IReadOnlyList<HitScopedProtectionBudgetRouteBatchItemResult>
        Items
    { get; }

    internal HitScopedProtectionBudgetRouteBatchExecutionResult(
        ProtectionAssignmentRoutingState previousState,
        ProtectionAssignmentRoutingState updatedState,
        HitScopedProtectionBudgetRouteBatchItemResult[] items)
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