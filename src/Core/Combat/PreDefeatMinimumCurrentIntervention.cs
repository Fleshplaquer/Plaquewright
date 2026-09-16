using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class PreDefeatMinimumCurrentIntervention
{
    public static PreDefeatMinimumCurrentInterventionResult Apply(
        PreDefeatInterventionContext context,
        ResourceId resourceId,
        double minimumCurrent)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ValidateMinimumCurrent(
            minimumCurrent);

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
                $"Resource '{definition.Key}' is not defeat-relevant and cannot be used by this pre-defeat minimum-current intervention.");
        }

        var evaluationBefore =
            EvaluateCurrent(
                context);

        if (!evaluationBefore.IsProjectedDefeated)
        {
            throw new InvalidOperationException(
                "Pre-defeat minimum-current intervention cannot be applied because the entity is no longer projected defeated.");
        }

        var valuesBefore =
            context.Draft.GetProjectedValues(
                resourceTarget);

        if (minimumCurrent >
            valuesBefore.Maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumCurrent),
                minimumCurrent,
                "Minimum current cannot exceed the projected resource maximum.");
        }

        var requestedRecovery =
            minimumCurrent >
            valuesBefore.Current
                ? minimumCurrent -
                  valuesBefore.Current
                : 0d;

        if (requestedRecovery > 0d)
        {
            var provenance =
                context.GameplayExecutionId.HasValue
                    ? new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery,
                        context.GameplayExecutionId.Value)
                    : new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery);

            var request =
                new ResourceRecoveryRequest(
                    resourceTarget.ResourceId,
                    requestedRecovery,
                    provenance);

            context.Draft.StageRecovery(
                resourceTarget,
                request);
        }

        var valuesAfter =
            context.Draft.GetProjectedValues(
                resourceTarget);

        var evaluationAfter =
            EvaluateCurrent(
                context);

        return new PreDefeatMinimumCurrentInterventionResult(
            context,
            resourceTarget,
            minimumCurrent,
            requestedRecovery,
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

    private static void ValidateMinimumCurrent(
        double minimumCurrent)
    {
        if (!double.IsFinite(
                minimumCurrent) ||
            minimumCurrent < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumCurrent),
                minimumCurrent,
                "Minimum current must be finite and non-negative.");
        }
    }
}
