namespace Idler.Core.Combat;

internal static class ProjectedEntityDefeatEvaluator
{
    public static ProjectedEntityDefeatEvaluation Evaluate(
        ProjectedEntityDefeatObservation observation,
        DefeatRelevantResourcePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(
            observation);

        ValidatePolicy(
            policy);

        var wasOriginallyDefeated =
            EvaluateOriginal(
                observation,
                policy);

        var isProjectedDefeated =
            EvaluateProjected(
                observation,
                policy);

        return new ProjectedEntityDefeatEvaluation(
            observation,
            policy,
            wasOriginallyDefeated,
            isProjectedDefeated);
    }

    private static bool EvaluateOriginal(
        ProjectedEntityDefeatObservation observation,
        DefeatRelevantResourcePolicy policy)
    {
        if (observation.DefeatRelevantResourceCount == 0)
        {
            return false;
        }

        return policy switch
        {
            DefeatRelevantResourcePolicy.AnyDepleted =>
                HasAnyOriginalDepletion(
                    observation),

            DefeatRelevantResourcePolicy.AllDepleted =>
                HasAllOriginalDepleted(
                    observation),

            _ =>
                throw new InvalidOperationException(
                    "Unsupported defeat relevant resource policy.")
        };
    }

    private static bool EvaluateProjected(
        ProjectedEntityDefeatObservation observation,
        DefeatRelevantResourcePolicy policy)
    {
        if (observation.DefeatRelevantResourceCount == 0)
        {
            return false;
        }

        return policy switch
        {
            DefeatRelevantResourcePolicy.AnyDepleted =>
                observation.HasAnyProjectedDepletion,

            DefeatRelevantResourcePolicy.AllDepleted =>
                observation.ProjectedDepletedResourceCount ==
                observation.DefeatRelevantResourceCount,

            _ =>
                throw new InvalidOperationException(
                    "Unsupported defeat relevant resource policy.")
        };
    }

    private static bool HasAnyOriginalDepletion(
        ProjectedEntityDefeatObservation observation)
    {
        foreach (var resource in observation.Resources)
        {
            if (resource.OriginalCurrent == 0d)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAllOriginalDepleted(
        ProjectedEntityDefeatObservation observation)
    {
        foreach (var resource in observation.Resources)
        {
            if (resource.OriginalCurrent != 0d)
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidatePolicy(
        DefeatRelevantResourcePolicy policy)
    {
        if (policy !=
                DefeatRelevantResourcePolicy.AnyDepleted &&
            policy !=
                DefeatRelevantResourcePolicy.AllDepleted)
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                policy,
                "Unknown defeat relevant resource policy.");
        }
    }
}