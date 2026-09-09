namespace Idler.Core.Resources;

public sealed class ResourceCostLedgerEntry
    : ResourceOperationLedgerEntry
{
    public ResourceCostRequest Request { get; }

    public ResourceCostResult Result { get; }

    internal ResourceCostLedgerEntry(
        ResourceCostRequest request,
        ResourceCostResult result)
        : base(
            request.ResourceId,
            request.Provenance)
    {
        if (request.ResourceId !=
            result.ResourceId)
        {
            throw new ArgumentException(
                "Cost request and result must target the same resource.");
        }

        if (request.Amount !=
            result.RequestedCost)
        {
            throw new ArgumentException(
                "Cost request amount must equal result requested cost.");
        }

        Request = request;
        Result = result;
    }
}