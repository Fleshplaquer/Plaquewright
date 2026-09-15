using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class DefeatAwareResourceTransactionCommitter
{
    public static DefeatAwareResourceTransactionCommitResult Commit(
        ResourceTransactionDraft draft,
        ResourceOperationLedger ledger,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        PreDefeatInterventionPhaseResult? preDefeatPhaseResult = null)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            ledger);

        ArgumentNullException.ThrowIfNull(
            entity);

        if (!ReferenceEquals(
                draft.ResourceRegistry,
                entity.ResourceRegistry))
        {
            throw new ArgumentException(
                "Entity belongs to a different resource registry than the transaction draft.",
                nameof(entity));
        }

        ValidateSingleOwnerDraft(
            draft,
            entity);

        var currentEvaluation =
            EvaluateCurrent(
                draft,
                entity,
                policy);

        DefeatAwareResourceTransactionCommitOutcome outcome;

        if (preDefeatPhaseResult is null)
        {
            if (currentEvaluation.IsNewDefeatTransition)
            {
                throw new InvalidOperationException(
                    "A new projected defeat transition must pass through the pre-defeat intervention phase before commit.");
            }

            outcome =
                DefeatAwareResourceTransactionCommitOutcome
                    .NoDefeatTransition;
        }
        else
        {
            ValidatePhaseResult(
                draft,
                entity,
                policy,
                currentEvaluation,
                preDefeatPhaseResult);

            outcome =
                preDefeatPhaseResult.Outcome switch
                {
                    PreDefeatInterventionPhaseOutcome.Resolved =>
                        DefeatAwareResourceTransactionCommitOutcome
                            .DefeatPrevented,

                    PreDefeatInterventionPhaseOutcome.Unresolved =>
                        DefeatAwareResourceTransactionCommitOutcome
                            .DefeatAccepted,

                    _ =>
                        throw new InvalidOperationException(
                            "Unsupported pre-defeat intervention phase outcome.")
                };
        }

        // All defeat-related validation happens before
        // the atomic resource transaction is committed.
        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        return new DefeatAwareResourceTransactionCommitResult(
            entity.Id,
            currentEvaluation,
            preDefeatPhaseResult,
            outcome);
    }

    private static void ValidateSingleOwnerDraft(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity)
    {
        foreach (var operation in draft.Operations)
        {
            var operationEntityId =
                operation switch
                {
                    StagedResourceLossOperation loss =>
                        loss.EntityId,

                    StagedResourceCostOperation cost =>
                        cost.EntityId,

                    StagedResourceRecoveryOperation recovery =>
                        recovery.EntityId,

                    _ =>
                        throw new InvalidOperationException(
                            $"Unsupported staged resource operation type '{operation.GetType().FullName}'.")
                };

            if (operationEntityId !=
                entity.Id)
            {
                throw new InvalidOperationException(
                    "The defeat-aware resource transaction commit currently supports only single-owner drafts. " +
                    $"The draft contains an operation for entity {operationEntityId}, " +
                    $"but the commit gate is evaluating entity {entity.Id}.");
            }
        }
    }

    private static ProjectedEntityDefeatEvaluation EvaluateCurrent(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy)
    {
        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                entity);

        return ProjectedEntityDefeatEvaluator.Evaluate(
            observation,
            policy);
    }

    private static void ValidatePhaseResult(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        ProjectedEntityDefeatEvaluation currentEvaluation,
        PreDefeatInterventionPhaseResult phaseResult)
    {
        if (!ReferenceEquals(
                phaseResult.Context.Draft,
                draft))
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result belongs to a different resource transaction draft.");
        }

        if (phaseResult.DraftVersion !=
            draft.Version)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result is stale because the resource transaction draft changed after phase finalization.");
        }

        if (!ReferenceEquals(
                phaseResult.Context.Entity,
                entity))
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result belongs to a different entity.");
        }

        if (phaseResult.Context.Policy !=
            policy)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result uses a different defeat policy.");
        }

        if (phaseResult.FinalEvaluation.IsProjectedDefeated !=
            currentEvaluation.IsProjectedDefeated)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result no longer matches the current projected defeat state.");
        }

        if (phaseResult.FinalEvaluation.IsNewDefeatTransition !=
            currentEvaluation.IsNewDefeatTransition)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase result no longer matches the current projected defeat transition.");
        }
    }
}