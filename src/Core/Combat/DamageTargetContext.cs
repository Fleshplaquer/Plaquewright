using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class DamageTargetContext
{
    internal SimulationRuntimeIdentity? RuntimeIdentity { get; }

    public DamageExecutionId DamageExecutionId { get; }

    public EntityId TargetEntityId { get; }

    public HitExecutionId? RelatedHitExecutionId { get; }

    public bool IsHitBased =>
        RelatedHitExecutionId.HasValue;

    internal DamageTargetContext(
        DamageExecutionId damageExecutionId,
        EntityId targetEntityId,
        HitExecutionId? relatedHitExecutionId)
    {
        Validate(
            damageExecutionId,
            targetEntityId,
            relatedHitExecutionId);

        DamageExecutionId =
            damageExecutionId;

        TargetEntityId =
            targetEntityId;

        RelatedHitExecutionId =
            relatedHitExecutionId;

        RuntimeIdentity =
            null;
    }

    internal DamageTargetContext(
        DamageExecutionId damageExecutionId,
        EntityId targetEntityId,
        HitExecutionId? relatedHitExecutionId,
        SimulationRuntimeIdentity runtimeIdentity)
    {
        Validate(
            damageExecutionId,
            targetEntityId,
            relatedHitExecutionId);

        ArgumentNullException.ThrowIfNull(
            runtimeIdentity);

        DamageExecutionId =
            damageExecutionId;

        TargetEntityId =
            targetEntityId;

        RelatedHitExecutionId =
            relatedHitExecutionId;

        RuntimeIdentity =
            runtimeIdentity;
    }

    private static void Validate(
        DamageExecutionId damageExecutionId,
        EntityId targetEntityId,
        HitExecutionId? relatedHitExecutionId)
    {
        if (!damageExecutionId.IsValid)
        {
            throw new ArgumentException(
                "Damage execution ID must be valid.",
                nameof(damageExecutionId));
        }

        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }

        if (relatedHitExecutionId.HasValue &&
            !relatedHitExecutionId.Value.IsValid)
        {
            throw new ArgumentException(
                "Related hit execution ID must be valid when provided.",
                nameof(relatedHitExecutionId));
        }
    }
}