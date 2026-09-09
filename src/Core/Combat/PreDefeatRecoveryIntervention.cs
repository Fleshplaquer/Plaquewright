using Idler.Core.Resources;

namespace Idler.Core.Combat;

internal static class PreDefeatRecoveryIntervention
{
    public static PreDefeatRecoveryInterventionResult Apply(
        PreDefeatInterventionContext context,
        ResourceId resourceId,
        double recoveryAmount)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        var resourceTarget =
            new ResourceStateTarget(
                context.Entity,
                resourceId);

        var definition =
            resourceTarget.ResourceRegistry.GetDefinition(
                resourceTarget.ResourceId);

        if (!definition.HasRole(
                ResourceRole.DefeatRelevant))
        {
            throw new InvalidOperationException(
                $"Resource '{definition.Key}' is not defeat-relevant and cannot be used by this pre-defeat recovery intervention.");
        }

        var request =
            new ResourceRecoveryRequest(
                resourceTarget.ResourceId,
                recoveryAmount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var evaluationBefore =
            EvaluateCurrent(
                context);

        if (!evaluationBefore.IsProjectedDefeated)
        {
            throw new InvalidOperationException(
                "Pre-defeat recovery cannot be applied because the entity is no longer projected defeated.");
        }

        var valuesBefore =
            context.Draft.GetProjectedValues(
                resourceTarget);

        context.Draft.StageRecovery(
            resourceTarget,
            request);

        var valuesAfter =
            context.Draft.GetProjectedValues(
                resourceTarget);

        var evaluationAfter =
            EvaluateCurrent(
                context);

        return new PreDefeatRecoveryInterventionResult(
            context,
            resourceTarget,
            recoveryAmount,
            valuesBefore.Current,
            valuesAfter.Current,
            evaluationBefore,
            evaluationAfter);
    }

    private static ProjectedEntityDefeatEvaluation EvaluateCurrent(
        PreDefeatInterventionContext context)
    {
        var observation =
            ProjectedEntityDefeatObserver.Observe(
                context.Draft,
                context.Entity);

        return ProjectedEntityDefeatEvaluator.Evaluate(
            observation,
            context.Policy);
    }
}