namespace Idler.Core.Resources;

public sealed class ResourceLossLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceLossRequest Request { get; }

    public ResourceLossResult Result { get; }

    internal ResourceLossLedgerEntry(
        ResourceLossRequest request,
        ResourceLossResult result)
        : base(
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Loss request and result must target the same resource.");
        }

        if (request.Amount !=
            result.RequestedLoss)
        {
            throw new ArgumentException(
                "Loss request amount must equal result requested loss.");
        }

        Request = request;
        Result = result;
    }
}