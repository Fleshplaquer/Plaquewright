using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Simulation;

internal sealed class SimulationRuntimeStateSnapshot
{
    public SimulationSeed RootSeed { get; }

    public EntityRuntimeStateSetSnapshot Entities { get; }

    public EntityIdAllocatorSnapshot EntityIds { get; }

    public ExecutionIdAllocatorSnapshot ExecutionIds { get; }

    public HitExecutionIdAllocatorSnapshot HitExecutionIds { get; }

    public DamageExecutionIdAllocatorSnapshot DamageExecutionIds { get; }

    internal SimulationRuntimeStateSnapshot(
        SimulationSeed rootSeed,
        EntityRuntimeStateSetSnapshot entities,
        EntityIdAllocatorSnapshot entityIds,
        ExecutionIdAllocatorSnapshot executionIds,
        HitExecutionIdAllocatorSnapshot hitExecutionIds,
        DamageExecutionIdAllocatorSnapshot damageExecutionIds)
    {
        ArgumentNullException.ThrowIfNull(
            entities);

        RootSeed =
            rootSeed;

        Entities =
            entities;

        EntityIds =
            entityIds;

        ExecutionIds =
            executionIds;

        HitExecutionIds =
            hitExecutionIds;

        DamageExecutionIds =
            damageExecutionIds;
    }
}