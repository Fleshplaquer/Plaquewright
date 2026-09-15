using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class PreDefeatInterventionContextFactory
{
    public static PreDefeatInterventionContext? TryCreate(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            entity);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                policy);

        if (!evaluation.IsNewDefeatTransition)
        {
            return null;
        }

        return new PreDefeatInterventionContext(
            draft,
            entity,
            evaluation);
    }
}