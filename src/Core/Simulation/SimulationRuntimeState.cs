using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Simulation;

public sealed class SimulationRuntimeState
{
    private readonly EntityIdAllocator _entityIdAllocator =
        new();

    private readonly ExecutionIdAllocator _executionIdAllocator =
        new();

    public SimulationSeed RootSeed { get; }

    public CompiledResourceRegistry ResourceRegistry { get; }

    public EntityRuntimeStateSet Entities { get; }

    public SimulationRuntimeState(
        SimulationSeed rootSeed,
        CompiledResourceRegistry resourceRegistry)
    {
        ArgumentNullException.ThrowIfNull(
            resourceRegistry);

        RootSeed = rootSeed;
        ResourceRegistry = resourceRegistry;

        Entities =
            new EntityRuntimeStateSet(
                resourceRegistry);
    }

    public EntityRuntimeState CreateEntity(
        IEnumerable<ResourceState> initialResources)
    {
        ArgumentNullException.ThrowIfNull(
            initialResources);

        var entity =
            new EntityRuntimeState(
                _entityIdAllocator.Allocate(),
                ResourceRegistry,
                initialResources);

        Entities.Add(
            entity);

        return entity;
    }

    public GameplayExecutionContext CreateExecutionContext(
        EntityId sourceEntityId,
        SimulationTime startedAt)
    {
        if (!sourceEntityId.IsValid)
        {
            throw new ArgumentException(
                "Source entity ID must be valid.",
                nameof(sourceEntityId));
        }

        _ = Entities.Get(
            sourceEntityId);

        return new GameplayExecutionContext(
            _executionIdAllocator.Allocate(),
            sourceEntityId,
            startedAt,
            RootSeed);
    }
}