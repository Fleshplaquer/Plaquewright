using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal static class PreDefeatInterventionContextFactory
{
    public static PreDefeatInterventionContext? TryCreate(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy)
    {
        return TryCreateCore(
            draft,
            entity,
            policy,
            gameplayExecutionId: null);
    }

    // The caller supplies the execution responsible for the intervention,
    // which may differ from the execution that caused the defeat transition.
    public static PreDefeatInterventionContext? TryCreate(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        ExecutionId gameplayExecutionId)
    {
        return TryCreateCore(
            draft,
            entity,
            policy,
            gameplayExecutionId);
    }

    private static PreDefeatInterventionContext? TryCreateCore(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        ExecutionId? gameplayExecutionId)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            entity);

        // Validate even when no new defeat transition would produce a context.
        if (gameplayExecutionId.HasValue &&
            !gameplayExecutionId.Value.IsValid)
        {
            throw new ArgumentException(
                "Gameplay execution ID must be valid when provided.",
                nameof(gameplayExecutionId));
        }

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
            evaluation,
            gameplayExecutionId);
    }
}
