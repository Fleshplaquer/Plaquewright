using Idler.Core.Entities;

namespace Idler.Core.Resources;

public abstract class ResourceOperationLedgerEntry
{
    public EntityId TargetEntityId { get; }

    public ResourceId ResourceId { get; }

    public ResourceOperationProvenance Provenance { get; }

    protected ResourceOperationLedgerEntry(
        EntityId targetEntityId,
        ResourceId resourceId,
        ResourceOperationProvenance provenance)
    {
        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }

        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        TargetEntityId =
            targetEntityId;

        ResourceId =
            resourceId;

        Provenance =
            provenance;
    }
}