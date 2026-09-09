using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Combat;

internal static class ProjectedEntityDefeatObserver
{
    public static ProjectedEntityDefeatObservation Observe(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            entity);

        var observations =
            new List<ProjectedDefeatResourceObservation>();

        foreach (var state in entity.Resources.States)
        {
            var definition =
                entity.ResourceRegistry.GetDefinition(
                    state.Id);

            if (!definition.HasRole(
                    ResourceRole.DefeatRelevant))
            {
                continue;
            }

            var target =
                new ResourceStateTarget(
                    entity,
                    state.Id);

            var projectedValues =
                draft.GetProjectedValues(
                    target);

            var candidate =
                ProjectedDefeatCandidateDetector.Detect(
                    draft,
                    target);

            observations.Add(
                new ProjectedDefeatResourceObservation(
                    target,
                    projectedValues,
                    isNewDepletionCandidate:
                        candidate is not null));
        }

        return new ProjectedEntityDefeatObservation(
            entity.Id,
            observations.ToArray());
    }
}