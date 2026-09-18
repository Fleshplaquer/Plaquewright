using Plaquewright.Core.Entities;
using Plaquewright.Core.Events;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class DamageCommittedEvent
    : IDomainEvent
{
    public DamageExecutionId DamageExecutionId { get; }

    public ExecutionId GameplayExecutionId { get; }

    public EntityId SourceEntityId { get; }

    public EntityId TargetEntityId { get; }

    public HitExecutionId? RelatedHitExecutionId { get; }

    public bool IsHitBased =>
        RelatedHitExecutionId.HasValue;

    public SimulationTime StartedAt { get; }

    public DefeatAwareResourceTransactionCommitOutcome
        Outcome
    { get; }

    public bool DefeatWasPrevented =>
        Outcome ==
        DefeatAwareResourceTransactionCommitOutcome
            .DefeatPrevented;

    public bool DefeatWasAccepted =>
        Outcome ==
        DefeatAwareResourceTransactionCommitOutcome
            .DefeatAccepted;

    internal DamageCommittedEvent(
        DamageResolutionContext resolution,
        DefeatAwareResourceTransactionCommitResult commitResult)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        ArgumentNullException.ThrowIfNull(
            commitResult);

        if (resolution.TargetEntityId !=
            commitResult.EntityId)
        {
            throw new InvalidOperationException(
                "Damage resolution and committed defeat result belong to different target entities.");
        }

        DamageExecutionId =
            resolution.DamageExecutionId;

        GameplayExecutionId =
            resolution.GameplayExecutionId;

        SourceEntityId =
            resolution.SourceEntityId;

        TargetEntityId =
            resolution.TargetEntityId;

        RelatedHitExecutionId =
            resolution.RelatedHitExecutionId;

        StartedAt =
            resolution.StartedAt;

        Outcome =
            commitResult.Outcome;
    }
}