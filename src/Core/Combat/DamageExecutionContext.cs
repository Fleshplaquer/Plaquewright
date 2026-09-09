using Idler.Core.Entities;
using Idler.Core.Simulation;

namespace Idler.Core.Combat;

public sealed class DamageExecutionContext
{
    public DamageExecutionId Id { get; }

    public ExecutionId GameplayExecutionId { get; }

    public EntityId SourceEntityId { get; }

    public SimulationTime StartedAt { get; }

    internal DamageExecutionContext(
        DamageExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        SimulationTime startedAt)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Damage execution ID must be valid.",
                nameof(id));
        }

        if (!gameplayExecutionId.IsValid)
        {
            throw new ArgumentException(
                "Gameplay execution ID must be valid.",
                nameof(gameplayExecutionId));
        }

        if (!sourceEntityId.IsValid)
        {
            throw new ArgumentException(
                "Source entity ID must be valid.",
                nameof(sourceEntityId));
        }

        Id =
            id;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        StartedAt =
            startedAt;
    }
}