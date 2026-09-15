namespace Plaquewright.Core.Combat;

internal static class PreDefeatInterventionPhaseFinalizer
{
    public static PreDefeatInterventionPhaseResult Finalize(
        PreDefeatInterventionContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                context.Draft,
                context.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                context.Policy);

        var outcome =
            evaluation.IsProjectedDefeated
                ? PreDefeatInterventionPhaseOutcome.Unresolved
                : PreDefeatInterventionPhaseOutcome.Resolved;

        return new PreDefeatInterventionPhaseResult(
            context,
            evaluation,
            outcome);
    }
}