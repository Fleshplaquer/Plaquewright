using Idler.Core.Entities;

namespace Idler.Core.Resources;

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
        if (request.ResourceId != result.ResourceId)
        {
            throw new ArgumentException(
                "Cost request and result must refer to the same resource.",
                nameof(result));
        }

        Request =
            request;

        Result =
            result;
    }
}