using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Combat;

public sealed class ProjectedEntityDefeatObservation
{
    private readonly ProjectedDefeatResourceObservation[] _resources;

    public EntityId EntityId { get; }

    public IReadOnlyList<ProjectedDefeatResourceObservation> Resources { get; }

    public int DefeatRelevantResourceCount =>
        _resources.Length;

    public int ProjectedDepletedResourceCount
    {
        get
        {
            var count =
                0;

            foreach (var resource in _resources)
            {
                if (resource.IsProjectedDepleted)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int NewDepletionCandidateCount
    {
        get
        {
            var count =
                0;

            foreach (var resource in _resources)
            {
                if (resource.IsNewDepletionCandidate)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public bool HasAnyProjectedDepletion =>
        ProjectedDepletedResourceCount > 0;

    public bool HasAnyNewDepletionCandidate =>
        NewDepletionCandidateCount > 0;

    internal ProjectedEntityDefeatObservation(
        EntityId entityId,
        ProjectedDefeatResourceObservation[] resources)
    {
        if (entityId == default)
        {
            throw new ArgumentException(
                "Entity ID must be valid.",
                nameof(entityId));
        }

        ArgumentNullException.ThrowIfNull(
            resources);

        EntityId =
            entityId;

        _resources =
            resources;

        Resources =
            Array.AsReadOnly(
                _resources);
    }
}