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
    // Only rehydrates facts captured from an existing committed event.
    // The live construction path above still requires the real commit result.
    internal DamageCommittedEvent(
        DamageCommittedEventSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DamageExecutionId = snapshot.DamageExecutionId;
        GameplayExecutionId = snapshot.GameplayExecutionId;
        SourceEntityId = snapshot.SourceEntityId;
        TargetEntityId = snapshot.TargetEntityId;
        RelatedHitExecutionId = snapshot.RelatedHitExecutionId;
        StartedAt = snapshot.StartedAt;
        Outcome = snapshot.Outcome;
    }
}