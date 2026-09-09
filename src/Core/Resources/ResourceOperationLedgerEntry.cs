namespace Idler.Core.Resources;

public abstract class ResourceOperationLedgerEntry
{
    public ResourceId ResourceId { get; }

    public ResourceOperationProvenance Provenance { get; }

    protected ResourceOperationLedgerEntry(
        ResourceId resourceId,
        ResourceOperationProvenance provenance)
    {
        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        if (!provenance.IsValid)
        {
            throw new ArgumentException(
                "Resource operation provenance must be valid.",
                nameof(provenance));
        }

        ResourceId = resourceId;
        Provenance = provenance;
    }
}