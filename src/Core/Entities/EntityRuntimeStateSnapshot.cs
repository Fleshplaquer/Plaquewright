using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Entities;

internal sealed class EntityRuntimeStateSnapshot
{
    public EntityId Id { get; }

    public ResourceStateSetSnapshot Resources { get; }

    private EntityRuntimeStateSnapshot(
        EntityId id,
        ResourceStateSetSnapshot resources)
    {
        Id =
            id;

        Resources =
            resources;
    }

    public static EntityRuntimeStateSnapshot Capture(
        EntityRuntimeState entity)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        return new EntityRuntimeStateSnapshot(
            entity.Id,
            ResourceStateSetSnapshot.Capture(
                entity.Resources));
    }

    public EntityRuntimeState Restore(
        CompiledResourceRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        var resources =
            Resources.Restore(
                registry);

        return new EntityRuntimeState(
            Id,
            resources);
    }
}