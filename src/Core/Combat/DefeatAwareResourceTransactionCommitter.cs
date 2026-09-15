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
            entity);

        var results =
            Commit(
                draft,
                ledger,
                [
                    new DefeatAwareResourceTransactionOwnerCommitRequest(
                        entity,
                        policy,
                        preDefeatPhaseResult)
                ]);

        return results[0];
    }

    public static IReadOnlyList<DefeatAwareResourceTransactionCommitResult>
        Commit(
            ResourceTransactionDraft draft,
            ResourceOperationLedger ledger,
            IReadOnlyList<DefeatAwareResourceTransactionOwnerCommitRequest>
                ownerRequests)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            ledger);

        ArgumentNullException.ThrowIfNull(
            ownerRequests);

        var requests =
            MaterializeRequests(
                ownerRequests);

        ValidateOwnerRequests(
            draft,
            requests);

        ValidateDraftOwnership(
            draft,
            requests);

        var results =
            new DefeatAwareResourceTransactionCommitResult[
                requests.Length];

        for (var index = 0;
             index < requests.Length;
             index++)
        {
            results[index] =
                EvaluateOwner(
                    draft,
                    requests[index]);
        }

        // Materialize the read-only result before committing.
        //
        // After the resource commit starts, no further
        // defeat validation is allowed to fail.
        var readOnlyResults =
            Array.AsReadOnly(
                results);

        // All owners have now passed the defeat gate.
        // The entire resource draft is still committed exactly once.
        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        return readOnlyResults;
    }

    private static DefeatAwareResourceTransactionOwnerCommitRequest[]
        MaterializeRequests(
            IReadOnlyList<
                DefeatAwareResourceTransactionOwnerCommitRequest>
                ownerRequests)
    {
        var requests =
            new DefeatAwareResourceTransactionOwnerCommitRequest[
                ownerRequests.Count];

        for (var index = 0;
             index < ownerRequests.Count;
             index++)
        {
            var request =
                ownerRequests[index];

            if (request is null)
            {
                throw new ArgumentException(
                    "Defeat-aware owner requests cannot contain null entries.",
                    nameof(ownerRequests));
            }

            requests[index] =
                request;
        }

        return requests;
    }

    private static void ValidateOwnerRequests(
        ResourceTransactionDraft draft,
        IReadOnlyList<
            DefeatAwareResourceTransactionOwnerCommitRequest>
            requests)
    {
        for (var index = 0;
             index < requests.Count;
             index++)
        {
            var request =
                requests[index];

            if (!ReferenceEquals(
                    draft.ResourceRegistry,
                    request.Entity.ResourceRegistry))
            {
                throw new ArgumentException(
                    "Entity belongs to a different resource registry than the transaction draft.",
                    nameof(requests));
            }

            for (var otherIndex = 0;
                 otherIndex < index;
                 otherIndex++)
            {
                if (requests[otherIndex].Entity.Id ==
                    request.Entity.Id)
                {
                    throw new InvalidOperationException(
                        $"Defeat-aware transaction contains more than one owner request for entity {request.Entity.Id}.");
                }
            }
        }
    }

    private static void ValidateDraftOwnership(
        ResourceTransactionDraft draft,
        IReadOnlyList<
            DefeatAwareResourceTransactionOwnerCommitRequest>
            requests)
    {
        foreach (var projection in draft.Projections)
        {
            var owningRequestIndex =
                FindOwningRequestIndex(
                    requests,
                    projection.OriginalState);

            if (owningRequestIndex < 0)
            {
                throw new InvalidOperationException(
                    "The resource transaction draft contains a projected resource state whose owning entity was not supplied to the defeat-aware commit gate.");
            }
        }

        foreach (var operation in draft.Operations)
        {
            var entityId =
                GetOperationEntityId(
                    operation);

            if (!ContainsEntityId(
                    requests,
                    entityId))
            {
                throw new InvalidOperationException(
                    $"The resource transaction draft contains an operation for entity {entityId}, but that entity was not supplied to the defeat-aware commit gate.");
            }
        }
    }

    private static int FindOwningRequestIndex(
        IReadOnlyList<
            DefeatAwareResourceTransactionOwnerCommitRequest>
            requests,
        ResourceState state)
    {
        var matchIndex =
            -1;

        for (var requestIndex = 0;
             requestIndex < requests.Count;
             requestIndex++)
        {
            if (!EntityOwnsState(
                    requests[requestIndex].Entity,
                    state))
            {
                continue;
            }

            if (matchIndex >= 0)
            {
                throw new InvalidOperationException(
                    "A projected resource state is owned by more than one supplied entity.");
            }

            matchIndex =
                requestIndex;
        }

        return matchIndex;
    }

    private static bool EntityOwnsState(
        EntityRuntimeState entity,
        ResourceState state)
    {
        foreach (var entityState in entity.Resources.States)
        {
            if (ReferenceEquals(
                    entityState,
                    state))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsEntityId(
        IReadOnlyList<
            DefeatAwareResourceTransactionOwnerCommitRequest>
            requests,
        EntityId entityId)
    {
        foreach (var request in requests)
        {
            if (request.Entity.Id ==
                entityId)
            {
                return true;
            }
        }

        return false;
    }

    private static EntityId GetOperationEntityId(
        StagedResourceOperation operation)
    {
        ArgumentNullException.ThrowIfNull(
            operation);

        return operation switch
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
    }

    private static DefeatAwareResourceTransactionCommitResult
        EvaluateOwner(
            ResourceTransactionDraft draft,
            DefeatAwareResourceTransactionOwnerCommitRequest request)
    {
        var entity =
            request.Entity;

        var currentEvaluation =
            EvaluateCurrent(
                draft,
                entity,
                request.Policy);

        DefeatAwareResourceTransactionCommitOutcome outcome;

        if (request.PreDefeatPhaseResult is null)
        {
            if (currentEvaluation.IsNewDefeatTransition)
            {
                throw new InvalidOperationException(
                    $"Entity {entity.Id} has a new projected defeat transition that must pass through the pre-defeat intervention phase before commit.");
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
                request.Policy,
                currentEvaluation,
                request.PreDefeatPhaseResult);

            outcome =
                request.PreDefeatPhaseResult.Outcome switch
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

        return new DefeatAwareResourceTransactionCommitResult(
            entity.Id,
            currentEvaluation,
            request.PreDefeatPhaseResult,
            outcome);
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