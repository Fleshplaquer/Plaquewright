using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Simulation;

public sealed class SimulationRuntimeState
{
    private readonly EntityIdAllocator _entityIdAllocator =
        new();

    private readonly ExecutionIdAllocator _executionIdAllocator =
        new();

    private readonly HitExecutionIdAllocator
        _hitExecutionIdAllocator =
            new();

    private readonly DamageExecutionIdAllocator
        _damageExecutionIdAllocator =
            new();

    internal HitExecutionId AllocateHitExecutionId()
    {
        return _hitExecutionIdAllocator.Allocate();
    }

    internal DamageExecutionId AllocateDamageExecutionId()
    {
        return _damageExecutionIdAllocator.Allocate();
    }
    internal HitExecutionContext CreateHitExecutionContext(
    GameplayExecutionContext gameplayExecution,
    EntityId targetEntityId,
    SimulationTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(
            gameplayExecution);

        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }

        // Both participants must belong to the runtime's
        // current entity state.
        Entities.Get(
            gameplayExecution.SourceEntityId);

        Entities.Get(
            targetEntityId);

        // Validation happens before allocation so failed
        // creation attempts do not consume deterministic IDs.
        var id =
            AllocateHitExecutionId();

        return new HitExecutionContext(
            id,
            gameplayExecution.Id,
            gameplayExecution.SourceEntityId,
            targetEntityId,
            startedAt);
    }

    internal DamageExecutionContext CreateDamageExecutionContext(
    GameplayExecutionContext gameplayExecution,
    SimulationTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(
            gameplayExecution);

        // The source must still belong to the runtime.
        Entities.Get(
            gameplayExecution.SourceEntityId);

        // Validation happens before deterministic ID allocation.
        var id =
            AllocateDamageExecutionId();

        return new DamageExecutionContext(
            id,
            gameplayExecution.Id,
            gameplayExecution.SourceEntityId,
            startedAt);
    }

    internal DamageTargetContext CreateDamageTargetContext(
    DamageExecutionContext damageExecution,
    EntityId targetEntityId)
    {
        ArgumentNullException.ThrowIfNull(
            damageExecution);

        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }

        Entities.Get(
            damageExecution.SourceEntityId);

        Entities.Get(
            targetEntityId);

        return new DamageTargetContext(
            damageExecution.Id,
            targetEntityId,
            relatedHitExecutionId: null);
    }

    internal DamageTargetContext CreateDamageTargetContext(
    DamageExecutionContext damageExecution,
    HitExecutionContext hitExecution)
    {
        ArgumentNullException.ThrowIfNull(
            damageExecution);

        ArgumentNullException.ThrowIfNull(
            hitExecution);

        Entities.Get(
            damageExecution.SourceEntityId);

        Entities.Get(
            hitExecution.TargetEntityId);

        if (damageExecution.GameplayExecutionId !=
            hitExecution.GameplayExecutionId)
        {
            throw new InvalidOperationException(
                "Damage execution and hit execution belong to different gameplay executions.");
        }

        if (damageExecution.SourceEntityId !=
            hitExecution.SourceEntityId)
        {
            throw new InvalidOperationException(
                "Damage execution and hit execution have different source entities.");
        }

        return new DamageTargetContext(
            damageExecution.Id,
            hitExecution.TargetEntityId,
            hitExecution.Id);
    }

    internal DamageResolutionContext CreateDamageResolutionContext(
    DamageExecutionContext damageExecution,
    DamageTargetContext damageTarget,
    DamageResolutionQuantities quantities)
    {
        ArgumentNullException.ThrowIfNull(
            damageExecution);

        ArgumentNullException.ThrowIfNull(
            damageTarget);

        ArgumentNullException.ThrowIfNull(
            quantities);

        Entities.Get(
            damageExecution.SourceEntityId);

        Entities.Get(
            damageTarget.TargetEntityId);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

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

        //
        // Build and validate the complete resource state
        // before consuming a deterministic entity ID.
        //
        var resources =
            new ResourceStateSet(
                ResourceRegistry,
                initialResources);

        var id =
            _entityIdAllocator.Allocate();

        var entity =
            new EntityRuntimeState(
                id,
                resources);

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