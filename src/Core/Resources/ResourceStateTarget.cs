using Idler.Core.Entities;

namespace Idler.Core.Resources;

public sealed class ResourceStateTarget
{
    public EntityId EntityId { get; }

    public CompiledResourceRegistry ResourceRegistry { get; }

    public ResourceState State { get; }

    public ResourceId ResourceId =>
        State.Id;

    public ResourceStateTarget(
        EntityRuntimeState entity,
        ResourceId resourceId)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        EntityId =
            entity.Id;

        ResourceRegistry =
            entity.ResourceRegistry;

        State =
            entity.Resources.Get(
                resourceId);
    }
}