using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public abstract class ResourceOperationLedgerEntry
{
    public EntityId TargetEntityId { get; }

    public ResourceId ResourceId { get; }

    public ResourceOperationProvenance Provenance { get; }

    internal ResourceOperationLedgerEntry(
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

        if (!provenance.IsValid)
        {
            throw new ArgumentException(
                "Resource operation provenance must be valid.",
                nameof(provenance));
        }

        TargetEntityId =
            targetEntityId;

        ResourceId =
            resourceId;

        Provenance =
            provenance;
    }
}