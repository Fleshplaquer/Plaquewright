using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class ResolvedDamageApplicationExecutor
{
    public static ResolvedDamageApplicationResult Apply(
        DamageResolutionContext resolution,
        IReadOnlyList<DamageResourceLossPlan>
            resourceLossPlans,
        IReadOnlyList<DamageApplicationOwnerPlan>
            ownerPlans,
        ResourceOperationLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        ArgumentNullException.ThrowIfNull(
            resourceLossPlans);

        ArgumentNullException.ThrowIfNull(
            ownerPlans);

        ArgumentNullException.ThrowIfNull(
            ledger);

        if (ownerPlans.Count == 0)
        {
            throw new ArgumentException(
                "Resolved damage application requires at least one owner plan.",
                nameof(ownerPlans));
        }

        var registry =
            ownerPlans[0].Entity.ResourceRegistry;

        var draft =
            new ResourceTransactionDraft(
                registry);

        var previews =
            new ResourceLossPreview[
                resourceLossPlans.Count];

        for (var index = 0;
             index < resourceLossPlans.Count;
             index++)
        {
            var plan =
                resourceLossPlans[index];

            if (plan is null)
            {
                throw new ArgumentException(
                    "Damage resource loss plans cannot contain null entries.",
                    nameof(resourceLossPlans));
            }

            ValidatePlanBelongsToResolution(
                resolution,
                plan);

            previews[index] =
                DamageResourceTransactionStager.Stage(
                    draft,
                    plan);
        }

        var ownerRequests =
            new DefeatAwareResourceTransactionOwnerCommitRequest[
                ownerPlans.Count];

        for (var index = 0;
             index < ownerPlans.Count;
             index++)
        {
            var ownerPlan =
                ownerPlans[index];

            if (ownerPlan is null)
            {
                throw new ArgumentException(
                    "Damage application owner plans cannot contain null entries.",
                    nameof(ownerPlans));
            }

            var phaseResult =
                ResolvePreDefeatPhase(
                    draft,
                    ownerPlan);

            ownerRequests[index] =
                new DefeatAwareResourceTransactionOwnerCommitRequest(
                    ownerPlan.Entity,
                    ownerPlan.Policy,
                    phaseResult);
        }

        var commitResults =
            DefeatAwareResourceTransactionCommitter
                .Commit(
                    draft,
                    ledger,
                    ownerRequests);

        var targetCommitResult =
            FindTargetCommitResult(
                resolution,
                commitResults);

        return new ResolvedDamageApplicationResult(
            resolution,
            Array.AsReadOnly(
                previews),
            commitResults,
            targetCommitResult);
    }

    private static PreDefeatInterventionPhaseResult?
        ResolvePreDefeatPhase(
            ResourceTransactionDraft draft,
            DamageApplicationOwnerPlan ownerPlan)
    {
        PreDefeatInterventionContext? context;

        if (ownerPlan.PreDefeatGameplayExecutionId
            is { } executionId)
        {
            context =
                PreDefeatInterventionContextFactory
                    .TryCreate(
                        draft,
                        ownerPlan.Entity,
                        ownerPlan.Policy,
                        executionId);
        }
        else
        {
            context =
                PreDefeatInterventionContextFactory
                    .TryCreate(
                        draft,
                        ownerPlan.Entity,
                        ownerPlan.Policy);
        }

        if (context is null)
        {
            return null;
        }

        ownerPlan.ApplyPreDefeatInterventions(
            context);

        return PreDefeatInterventionPhaseFinalizer
            .Finalize(
                context);
    }

    private static void ValidatePlanBelongsToResolution(
        DamageResolutionContext resolution,
        DamageResourceLossPlan plan)
    {
        var planResolution =
            plan.TargetContext.Resolution;

        if (planResolution.DamageExecutionId !=
                resolution.DamageExecutionId ||
            planResolution.GameplayExecutionId !=
                resolution.GameplayExecutionId ||
            planResolution.TargetEntityId !=
                resolution.TargetEntityId)
        {
            throw new InvalidOperationException(
                "Damage resource loss plan belongs to a different damage resolution.");
        }
    }

    private static DefeatAwareResourceTransactionCommitResult
        FindTargetCommitResult(
            DamageResolutionContext resolution,
            IReadOnlyList<
                DefeatAwareResourceTransactionCommitResult>
                commitResults)
    {
        DefeatAwareResourceTransactionCommitResult?
            targetResult =
                null;

        for (var index = 0;
             index < commitResults.Count;
             index++)
        {
            var result =
                commitResults[index];

            if (result.EntityId !=
                resolution.TargetEntityId)
            {
                continue;
            }

            if (targetResult is not null)
            {
                throw new InvalidOperationException(
                    "Resolved damage application produced more than one commit result for the damage target.");
            }

            targetResult =
                result;
        }

        return targetResult ??
               throw new InvalidOperationException(
                   "Resolved damage application produced no commit result for the damage target.");
    }
}