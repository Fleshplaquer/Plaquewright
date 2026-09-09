using Idler.Core.Resources;

namespace Idler.Core.Combat;

internal static class ProjectedDefeatCandidateDetector
{
    public static ProjectedDefeatCandidate? Detect(
        ResourceTransactionDraft draft,
        ResourceStateTarget resourceTarget)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        var definition =
            resourceTarget.ResourceRegistry.GetDefinition(
                resourceTarget.ResourceId);

        if (!definition.HasRole(
                ResourceRole.DefeatRelevant))
        {
            return null;
        }

        var originalCurrent =
            resourceTarget.State.Current;

        if (originalCurrent <= 0d)
        {
            return null;
        }

        var projectedValues =
            draft.GetProjectedValues(
                resourceTarget);

        if (projectedValues.Current != 0d)
        {
            return null;
        }

        return new ProjectedDefeatCandidate(
            resourceTarget,
            originalCurrent,
            projectedValues);
    }
}