using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Entities;

public sealed class EntityRuntimeState
{
    public EntityId Id { get; }

    public CompiledResourceRegistry ResourceRegistry { get; }

    public ResourceStateSet Resources { get; }

    public EntityRuntimeState(
        EntityId id,
        CompiledResourceRegistry resourceRegistry,
        IEnumerable<ResourceState> initialResources)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Entity ID must be valid.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(
            resourceRegistry);

        ArgumentNullException.ThrowIfNull(
            initialResources);

        Id = id;

        ResourceRegistry =
            resourceRegistry;

        Resources =
            new ResourceStateSet(
                resourceRegistry,
                initialResources);
    }

    internal EntityRuntimeState(
     EntityId id,
     ResourceStateSet resources)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Entity ID must be valid.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(
            resources);

        Id =
            id;

        ResourceRegistry =
            resources.ResourceRegistry;

        Resources =
            resources;
    }
}