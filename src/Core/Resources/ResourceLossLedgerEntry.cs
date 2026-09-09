using Idler.Core.Entities;

namespace Idler.Core.Resources;

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
        if (request.ResourceId != result.ResourceId)
        {
            throw new ArgumentException(
                "Loss request and result must refer to the same resource.",
                nameof(result));
        }

        Request =
            request;

        Result =
            result;
    }
}