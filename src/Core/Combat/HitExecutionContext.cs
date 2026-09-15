using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class HitExecutionContext
{
    internal SimulationRuntimeIdentity? RuntimeIdentity { get; }

    public HitExecutionId Id { get; }

    public ExecutionId GameplayExecutionId { get; }

    public EntityId SourceEntityId { get; }

    public EntityId TargetEntityId { get; }

    public SimulationTime StartedAt { get; }

    internal HitExecutionContext(
        HitExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        EntityId targetEntityId,
        SimulationTime startedAt)
    {
        Validate(
            id,
            gameplayExecutionId,
            sourceEntityId,
            targetEntityId);

        Id =
            id;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        TargetEntityId =
            targetEntityId;

        StartedAt =
            startedAt;

        RuntimeIdentity =
            null;
    }

    internal HitExecutionContext(
        HitExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        EntityId targetEntityId,
        SimulationTime startedAt,
        SimulationRuntimeIdentity runtimeIdentity)
    {
        Validate(
            id,
            gameplayExecutionId,
            sourceEntityId,
            targetEntityId);

        ArgumentNullException.ThrowIfNull(
            runtimeIdentity);

        Id =
            id;

        GameplayExecutionId =
            gameplayExecutionId;

        SourceEntityId =
            sourceEntityId;

        TargetEntityId =
            targetEntityId;

        StartedAt =
            startedAt;

        RuntimeIdentity =
            runtimeIdentity;
    }

    private static void Validate(
        HitExecutionId id,
        ExecutionId gameplayExecutionId,
        EntityId sourceEntityId,
        EntityId targetEntityId)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Hit execution ID must be valid.",
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

        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }
    }
}