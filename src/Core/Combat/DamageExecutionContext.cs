using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class DamageExecutionContext
{
    internal SimulationRuntimeIdentity? RuntimeIdentity { get; }

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
        Validate(
            id,
            gameplayExecutionId,
            sourceEntityId);

        Id =
            id;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        StartedAt =
            startedAt;

        RuntimeIdentity =
            null;
    }

    internal DamageExecutionContext(
        DamageExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        SimulationTime startedAt,
        SimulationRuntimeIdentity runtimeIdentity)
    {
        Validate(
            id,
            gameplayExecutionId,
            sourceEntityId);

        ArgumentNullException.ThrowIfNull(
            runtimeIdentity);

        Id =
            id;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        StartedAt =
            startedAt;

        RuntimeIdentity =
            runtimeIdentity;
    }

    private static void Validate(
        DamageExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId)
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
    }
}