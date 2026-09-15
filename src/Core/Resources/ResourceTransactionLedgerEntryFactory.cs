namespace Plaquewright.Core.Resources;

internal static class ResourceTransactionLedgerEntryFactory
{
    public static IReadOnlyList<ResourceOperationLedgerEntry> CreateEntries(
        ResourceTransactionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        var entries =
            new ResourceOperationLedgerEntry[
                draft.OperationCount];

        for (var index = 0;
             index < draft.OperationCount;
             index++)
        {
            entries[index] =
                CreateEntry(
                    draft.Operations[index]);
        }

        return Array.AsReadOnly(
            entries);
    }

    private static ResourceOperationLedgerEntry CreateEntry(
    StagedResourceOperation operation)
    {
        ArgumentNullException.ThrowIfNull(
            operation);

        return operation switch
        {
            StagedResourceLossOperation loss =>
                new ResourceLossLedgerEntry(
                    loss.EntityId,
                    loss.Preview.Request,
                    loss.Preview.Result),

            StagedResourceCostOperation cost =>
    new ResourceCostLedgerEntry(
        cost.EntityId,
        cost.Preview.Request,
        new ResourceCostResult(
            cost.Preview.Request.ResourceId,
            cost.Preview.Request.Amount,
            cost.Preview.ProjectedActualCost)),

            StagedResourceRecoveryOperation recovery =>
                new ResourceRecoveryLedgerEntry(
                    recovery.EntityId,
                    recovery.Preview.Request,
                    recovery.Preview.Result),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported staged resource operation type " +
                    $"'{operation.GetType().FullName}'.")
        };
    }
}