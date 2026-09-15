using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public sealed class ResourceCostLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceCostRequest Request { get; }

    public ResourceCostResult Result { get; }

    internal ResourceCostLedgerEntry(
        EntityId targetEntityId,
        ResourceCostRequest request,
        ResourceCostResult result)
        : base(
            targetEntityId,
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Cost request and result must refer to the same resource.",
                nameof(result));
        }

        if (request.Amount !=
            result.RequestedCost)
        {
            throw new ArgumentException(
                "Cost request amount must match the result requested cost.",
                nameof(result));
        }

        Request =
            request;

        Result =
            result;
    }
}