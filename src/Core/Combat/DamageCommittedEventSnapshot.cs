using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal sealed class DamageCommittedEventSnapshot
{
    public DamageExecutionId DamageExecutionId { get; }
    public ExecutionId GameplayExecutionId { get; }
    public EntityId SourceEntityId { get; }
    public EntityId TargetEntityId { get; }
    public HitExecutionId? RelatedHitExecutionId { get; }
    public SimulationTime StartedAt { get; }
    public DefeatAwareResourceTransactionCommitOutcome Outcome { get; }

    private DamageCommittedEventSnapshot(
        DamageCommittedEvent domainEvent)
    {
        DamageExecutionId = domainEvent.DamageExecutionId;
        GameplayExecutionId = domainEvent.GameplayExecutionId;
        SourceEntityId = domainEvent.SourceEntityId;
        TargetEntityId = domainEvent.TargetEntityId;
        RelatedHitExecutionId = domainEvent.RelatedHitExecutionId;
        StartedAt = domainEvent.StartedAt;
        Outcome = domainEvent.Outcome;
    }

    public static DamageCommittedEventSnapshot Capture(
        DamageCommittedEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new DamageCommittedEventSnapshot(domainEvent);
    }

    public DamageCommittedEvent Restore()
    {
        return new DamageCommittedEvent(this);
    }
}