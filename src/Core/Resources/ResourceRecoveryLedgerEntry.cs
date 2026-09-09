namespace Idler.Core.Resources;

public sealed class ResourceRecoveryLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceRecoveryRequest Request { get; }

    public ResourceRecoveryResult Result { get; }

    internal ResourceRecoveryLedgerEntry(
        ResourceRecoveryRequest request,
        ResourceRecoveryResult result)
        : base(
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Recovery request and result must target the same resource.");
        }

        if (request.Amount !=
            result.RequestedRecovery)
        {
            throw new ArgumentException(
                "Recovery request amount must equal result requested recovery.");
        }

        Request = request;
        Result = result;
    }
}