using Idler.Core.Entities;

namespace Idler.Core.Combat;

public sealed class ProjectedEntityDefeatEvaluation
{
    public ProjectedEntityDefeatObservation Observation { get; }

    public EntityId EntityId =>
        Observation.EntityId;

    public DefeatRelevantResourcePolicy Policy { get; }

    public bool WasOriginallyDefeated { get; }

    public bool IsProjectedDefeated { get; }

    public bool IsNewDefeatTransition =>
        !WasOriginallyDefeated &&
        IsProjectedDefeated;

    internal ProjectedEntityDefeatEvaluation(
        ProjectedEntityDefeatObservation observation,
        DefeatRelevantResourcePolicy policy,
        bool wasOriginallyDefeated,
        bool isProjectedDefeated)
    {
        ArgumentNullException.ThrowIfNull(
            observation);

        Observation =
            observation;

        Policy =
            policy;

        WasOriginallyDefeated =
            wasOriginallyDefeated;

        IsProjectedDefeated =
            isProjectedDefeated;
    }
}