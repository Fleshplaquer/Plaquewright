using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

internal abstract class StagedResourceOperation
{
    public ResourceStateTarget Target { get; }

    public EntityId EntityId =>
        Target.EntityId;

    public ResourceState OriginalState =>
        Target.State;

    public ResourceId ResourceId =>
        Target.ResourceId;

    public abstract ResourceOperationProvenance Provenance { get; }

    protected StagedResourceOperation(
        ResourceStateTarget target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        Target =
            target;
    }
}