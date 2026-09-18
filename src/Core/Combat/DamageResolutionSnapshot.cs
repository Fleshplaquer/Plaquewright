using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal sealed class DamageResolutionSnapshot
{
    public DamageExecutionId DamageExecutionId { get; }

    public ExecutionId GameplayExecutionId { get; }

    public EntityId SourceEntityId { get; }

    public EntityId TargetEntityId { get; }

    public HitExecutionId? RelatedHitExecutionId { get; }

    public SimulationTime StartedAt { get; }

    public DamageResolutionQuantitiesSnapshot
        Quantities
    { get; }

    private DamageResolutionSnapshot(
        DamageExecutionId damageExecutionId,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        EntityId targetEntityId,
        HitExecutionId? relatedHitExecutionId,
        SimulationTime startedAt,
        DamageResolutionQuantitiesSnapshot quantities)
    {
        DamageExecutionId =
            damageExecutionId;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        TargetEntityId =
            targetEntityId;

        RelatedHitExecutionId =
            relatedHitExecutionId;

        StartedAt =
            startedAt;

        Quantities =
            quantities;
    }

    public static DamageResolutionSnapshot Capture(
        DamageResolutionContext resolution)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        return new DamageResolutionSnapshot(
            resolution.DamageExecutionId,
            resolution.GameplayExecutionId,
            resolution.SourceEntityId,
            resolution.TargetEntityId,
            resolution.RelatedHitExecutionId,
            resolution.StartedAt,
            DamageResolutionQuantitiesSnapshot.Capture(
                resolution.Quantities));
    }

    public DamageResolutionContext Restore(
        SimulationRuntimeState runtime)
    {
        ArgumentNullException.ThrowIfNull(
            runtime);

        return runtime.RestoreDamageResolutionContext(
            DamageExecutionId,
            GameplayExecutionId,
            SourceEntityId,
            TargetEntityId,
            RelatedHitExecutionId,
            StartedAt,
            Quantities.Restore());
    }
}