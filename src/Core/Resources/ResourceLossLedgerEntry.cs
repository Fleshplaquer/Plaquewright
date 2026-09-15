using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public sealed class ResourceLossLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceLossRequest Request { get; }

    public ResourceLossResult Result { get; }

    internal ResourceLossLedgerEntry(
        EntityId targetEntityId,
        ResourceLossRequest request,
        ResourceLossResult result)
        : base(
            targetEntityId,
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Loss request and result must refer to the same resource.",
                nameof(result));
        }

        if (request.Amount !=
            result.RequestedLoss)
        {
            throw new ArgumentException(
                "Loss request amount must match the result requested loss.",
                nameof(result));
        }

        Request =
            request;

        Result =
            result;
    }
}