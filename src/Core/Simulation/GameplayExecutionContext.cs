using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Simulation;

public sealed class GameplayExecutionContext
{
    private readonly SimulationSeed _rootSeed;

    internal SimulationRuntimeIdentity? RuntimeIdentity { get; }

    public ExecutionId Id { get; }

    public EntityId SourceEntityId { get; }

    public SimulationTime StartedAt { get; }

    public GameplayExecutionContext(
        ExecutionId id,
        EntityId sourceEntityId,
        SimulationTime startedAt,
        SimulationSeed rootSeed)
    {
        ValidateIdentity(
            id,
            sourceEntityId);

        Id =
            id;

        SourceEntityId =
            sourceEntityId;

        StartedAt =
            startedAt;

        _rootSeed =
            rootSeed;

        RuntimeIdentity =
            null;
    }

    internal GameplayExecutionContext(
        ExecutionId id,
        EntityId sourceEntityId,
        SimulationTime startedAt,
        SimulationSeed rootSeed,
        SimulationRuntimeIdentity runtimeIdentity)
    {
        ValidateIdentity(
            id,
            sourceEntityId);

        ArgumentNullException.ThrowIfNull(
            runtimeIdentity);

        Id =
            id;

        SourceEntityId =
            sourceEntityId;

        StartedAt =
            startedAt;

        _rootSeed =
            rootSeed;

        RuntimeIdentity =
            runtimeIdentity;
    }

    public DeterministicRng CreateRngStream(
        RngDomainKey domain,
        RngAlgorithmVersion algorithmVersion =
            RngAlgorithmVersion.SplitMix64V1)
    {
        return DeterministicRngFactory.CreateExecutionStream(
            _rootSeed,
            domain,
            Id,
            algorithmVersion);
    }

    private static void ValidateIdentity(
        ExecutionId id,
        EntityId sourceEntityId)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Execution ID must be valid.",
                nameof(id));
        }

        if (!sourceEntityId.IsValid)
        {
            throw new ArgumentException(
                "Source entity ID must be valid.",
                nameof(sourceEntityId));
        }
    }
}