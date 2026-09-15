using Idler.Core.Entities;

namespace Idler.Core.Resources;

public sealed class ResourceRecoveryLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceRecoveryRequest Request { get; }

    public ResourceRecoveryResult Result { get; }

    internal ResourceRecoveryLedgerEntry(
        EntityId targetEntityId,
        ResourceRecoveryRequest request,
        ResourceRecoveryResult result)
        : base(
            targetEntityId,
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Recovery request and result must refer to the same resource.",
                nameof(result));
        }

        if (request.Amount !=
            result.RequestedRecovery)
        {
            throw new ArgumentException(
                "Recovery request amount must match the result requested recovery.",
                nameof(result));
        }

        Request =
            request;

        Result =
            result;
    }
}