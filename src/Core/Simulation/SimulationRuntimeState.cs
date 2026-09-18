using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Simulation;

public sealed class SimulationRuntimeState
{
    private readonly EntityIdAllocator
    _entityIdAllocator;

    private readonly ExecutionIdAllocator
        _executionIdAllocator;

    private readonly SimulationRuntimeIdentity
        _runtimeIdentity;

    private readonly HitExecutionIdAllocator
        _hitExecutionIdAllocator;

    private readonly DamageExecutionIdAllocator
        _damageExecutionIdAllocator;

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
        ValidateRuntimeOwnership(
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
    startedAt,
    _runtimeIdentity);
    }

    internal DamageExecutionContext CreateDamageExecutionContext(
    GameplayExecutionContext gameplayExecution,
    SimulationTime startedAt)
    {
        ValidateRuntimeOwnership(
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
    startedAt,
    _runtimeIdentity);
    }

    private void ValidateRuntimeOwnership(
    HitExecutionContext hitExecution)
    {
        ArgumentNullException.ThrowIfNull(
            hitExecution);

        if (!ReferenceEquals(
                hitExecution.RuntimeIdentity,
                _runtimeIdentity))
        {
            throw new InvalidOperationException(
                "Hit execution context belongs to a different simulation runtime or is not runtime-bound.");
        }
    }

    private void ValidateRuntimeOwnership(
    DamageTargetContext damageTarget)
    {
        ArgumentNullException.ThrowIfNull(
            damageTarget);

        if (!ReferenceEquals(
                damageTarget.RuntimeIdentity,
                _runtimeIdentity))
        {
            throw new InvalidOperationException(
                "Damage target context belongs to a different simulation runtime or is not runtime-bound.");
        }
    }

    private void ValidateRuntimeOwnership(
        DamageExecutionContext damageExecution)
    {
        ArgumentNullException.ThrowIfNull(
            damageExecution);

        if (!ReferenceEquals(
                damageExecution.RuntimeIdentity,
                _runtimeIdentity))
        {
            throw new InvalidOperationException(
                "Damage execution context belongs to a different simulation runtime or is not runtime-bound.");
        }
    }

    internal DamageTargetContext CreateDamageTargetContext(
    DamageExecutionContext damageExecution,
    EntityId targetEntityId)
    {
        ValidateRuntimeOwnership(
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
            relatedHitExecutionId: null,
            _runtimeIdentity);
    }

    internal DamageTargetContext CreateDamageTargetContext(
    DamageExecutionContext damageExecution,
    HitExecutionContext hitExecution)
    {
        ValidateRuntimeOwnership(
    damageExecution);

        ValidateRuntimeOwnership(
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
            hitExecution.Id,
            _runtimeIdentity);
    }

    internal DamageResolutionContext CreateDamageResolutionContext(
    DamageExecutionContext damageExecution,
    DamageTargetContext damageTarget,
    DamageResolutionQuantities quantities)
    {
        ValidateRuntimeOwnership(
    damageExecution);

        ValidateRuntimeOwnership(
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
    : this(
        rootSeed,
        resourceRegistry,
        new EntityRuntimeStateSet(
            resourceRegistry),
        new EntityIdAllocator(),
        new ExecutionIdAllocator(),
        new HitExecutionIdAllocator(),
        new DamageExecutionIdAllocator(),
        new SimulationRuntimeIdentity())
    {
    }
    private SimulationRuntimeState(
    SimulationSeed rootSeed,
    CompiledResourceRegistry resourceRegistry,
    EntityRuntimeStateSet entities,
    EntityIdAllocator entityIdAllocator,
    ExecutionIdAllocator executionIdAllocator,
    HitExecutionIdAllocator hitExecutionIdAllocator,
    DamageExecutionIdAllocator damageExecutionIdAllocator,
    SimulationRuntimeIdentity runtimeIdentity)
    {
        ArgumentNullException.ThrowIfNull(
            resourceRegistry);

        ArgumentNullException.ThrowIfNull(
            entities);

        ArgumentNullException.ThrowIfNull(
            entityIdAllocator);

        ArgumentNullException.ThrowIfNull(
            executionIdAllocator);

        ArgumentNullException.ThrowIfNull(
            hitExecutionIdAllocator);

        ArgumentNullException.ThrowIfNull(
            damageExecutionIdAllocator);

        ArgumentNullException.ThrowIfNull(
            runtimeIdentity);

        if (!ReferenceEquals(
                entities.ResourceRegistry,
                resourceRegistry))
        {
            throw new ArgumentException(
                "Entity runtime state belongs to a different resource registry.",
                nameof(entities));
        }

        RootSeed =
            rootSeed;

        ResourceRegistry =
            resourceRegistry;

        Entities =
            entities;

        _entityIdAllocator =
            entityIdAllocator;

        _executionIdAllocator =
            executionIdAllocator;

        _hitExecutionIdAllocator =
            hitExecutionIdAllocator;

        _damageExecutionIdAllocator =
            damageExecutionIdAllocator;

        _runtimeIdentity =
            runtimeIdentity;
    }
    internal SimulationRuntimeStateSnapshot CaptureSnapshot()
    {
        return new SimulationRuntimeStateSnapshot(
            RootSeed,
            EntityRuntimeStateSetSnapshot.Capture(
                Entities),
            _entityIdAllocator.CaptureSnapshot(),
            _executionIdAllocator.CaptureSnapshot(),
            _hitExecutionIdAllocator.CaptureSnapshot(),
            _damageExecutionIdAllocator.CaptureSnapshot());
    }

    internal static SimulationRuntimeState Restore(
        SimulationRuntimeStateSnapshot snapshot,
        CompiledResourceRegistry resourceRegistry)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        ArgumentNullException.ThrowIfNull(
            resourceRegistry);

        var entities =
            snapshot.Entities.Restore(
                resourceRegistry);

        return new SimulationRuntimeState(
            snapshot.RootSeed,
            resourceRegistry,
            entities,
            EntityIdAllocator.Restore(
                snapshot.EntityIds),
            ExecutionIdAllocator.Restore(
                snapshot.ExecutionIds),
            HitExecutionIdAllocator.Restore(
                snapshot.HitExecutionIds),
            DamageExecutionIdAllocator.Restore(
                snapshot.DamageExecutionIds),

            //
            // Runtime ownership is intentionally rebound.
            // A restored runtime is a new runtime instance.
            //
            new SimulationRuntimeIdentity());
    }

    internal DamageResolutionContext
    RestoreDamageResolutionContext(
        DamageExecutionId damageExecutionId,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        EntityId targetEntityId,
        HitExecutionId? relatedHitExecutionId,
        SimulationTime startedAt,
        DamageResolutionQuantities quantities)
    {
        ArgumentNullException.ThrowIfNull(
            quantities);

        //
        // A restored resolution may only refer to entities
        // that actually exist in this runtime.
        //
        Entities.Get(
            sourceEntityId);

        Entities.Get(
            targetEntityId);

        //
        // IDs already existed before the snapshot.
        // Rebinding must therefore NOT allocate new IDs.
        //
        var damageExecution =
            new DamageExecutionContext(
                damageExecutionId,
                gameplayExecutionId,
                sourceEntityId,
                startedAt,
                _runtimeIdentity);

        var damageTarget =
            new DamageTargetContext(
                damageExecutionId,
                targetEntityId,
                relatedHitExecutionId,
                _runtimeIdentity);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
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
    RootSeed,
    _runtimeIdentity);
    }
    private void ValidateRuntimeOwnership(
    GameplayExecutionContext gameplayExecution)
    {
        ArgumentNullException.ThrowIfNull(
            gameplayExecution);

        if (!ReferenceEquals(
                gameplayExecution.RuntimeIdentity,
                _runtimeIdentity))
        {
            throw new InvalidOperationException(
                "Gameplay execution context belongs to a different simulation runtime or is not runtime-bound.");
        }
    }
}