using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Combat;

public sealed class DamageTargetContext
{
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

        DamageExecutionId =
            damageExecutionId;

        TargetEntityId =
            targetEntityId;

        RelatedHitExecutionId =
            relatedHitExecutionId;
    }
}