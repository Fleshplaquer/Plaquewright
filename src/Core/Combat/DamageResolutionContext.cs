using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class DamageResolutionContext
{
    public DamageExecutionContext Execution { get; }

    public DamageTargetContext Target { get; }

    public DamageResolutionQuantities Quantities { get; }

    public DamageExecutionId DamageExecutionId =>
        Execution.Id;

    public ExecutionId GameplayExecutionId =>
        Execution.GameplayExecutionId;

    public EntityId SourceEntityId =>
        Execution.SourceEntityId;

    public EntityId TargetEntityId =>
        Target.TargetEntityId;

    public HitExecutionId? RelatedHitExecutionId =>
        Target.RelatedHitExecutionId;

    public bool IsHitBased =>
        Target.IsHitBased;

    public SimulationTime StartedAt =>
        Execution.StartedAt;

    internal DamageResolutionContext(
        DamageExecutionContext execution,
        DamageTargetContext target,
        DamageResolutionQuantities quantities)
    {
        ArgumentNullException.ThrowIfNull(
            execution);

        ArgumentNullException.ThrowIfNull(
            target);

        ArgumentNullException.ThrowIfNull(
            quantities);

        if (execution.Id !=
            target.DamageExecutionId)
        {
            throw new InvalidOperationException(
                "Damage execution and damage target belong to different damage executions.");
        }

        Execution =
            execution;

        Target =
            target;

        Quantities =
            quantities;
    }
}