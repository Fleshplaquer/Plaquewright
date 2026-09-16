using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class ProtectionResourceRouteBatchExecutor
{
    public static ProtectionResourceRouteBatchExecutionResult Stage(
        ResourceTransactionDraft draft,
        ProtectionAssignmentRoutingState state,
        IReadOnlyList<ProtectionResourceRouteBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            bindings);

        if (bindings.Count == 0)
        {
            throw new ArgumentException(
                "A shared protection resource batch requires at least one binding.",
                nameof(bindings));
        }

        state.ValidateResourceTransactionDraft(
            draft);

        var copiedBindings =
            new ProtectionResourceRouteBinding[
                bindings.Count];

        var financingPlans =
            new ResourceBackedProtectionFinancingPlan[
                bindings.Count];

        var requestedResourceUnits =
            new double[
                bindings.Count];

        ProtectionResourceRouteBinding?
            firstBinding = null;

        for (var index = 0;
             index < bindings.Count;
             index++)
        {
            var binding =
                bindings[index];

            if (binding is null)
            {
                throw new ArgumentException(
                    "Shared protection resource batch cannot contain null bindings.",
                    nameof(bindings));
            }

            copiedBindings[index] =
                binding;

            if (firstBinding is null)
            {
                firstBinding =
                    binding;
            }
            else if (!SharesConcreteResourceSource(
                         firstBinding,
                         binding))
            {
                throw new InvalidOperationException(
                    "All bindings in a shared protection resource batch must use the same concrete resource source.");
            }

            for (var previousIndex = 0;
                 previousIndex < index;
                 previousIndex++)
            {
                if (ReferenceEquals(
                        copiedBindings[previousIndex].Assignment,
                        binding.Assignment))
                {
                    throw new InvalidOperationException(
                        "A protection assignment cannot appear more than once in the same shared resource batch.");
                }
            }

            var lane =
                FindLane(
                    state,
                    binding.Assignment);

            if (lane is null)
            {
                throw new InvalidOperationException(
                    "Protection resource route binding does not belong to this routing state.");
            }

            if (lane.IsComplete)
            {
                throw new InvalidOperationException(
                    "Cannot execute another protection route for a completed assignment lane.");
            }

            if (lane.Chain.AppliedRouteCount ==
                int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Protection assignment lane cannot accept another route.");
            }

            var financingPlan =
                binding.CreateFinancingPlan(
                    lane,
                    state.GameplayExecutionId);

            financingPlans[index] =
                financingPlan;

            requestedResourceUnits[index] =
                financingPlan.Financing
                    .RequestedResourceUnits;
        }

        // Every successful StageLoss advances the draft once.
        // Reject version exhaustion before any batch mutation.
        if ((ulong)bindings.Count >
            ulong.MaxValue -
            draft.Version)
        {
            throw new InvalidOperationException(
                "Resource transaction draft version is exhausted.");
        }

        var resourceTarget =
            firstBinding!.ResourceTarget;

        var projectedResource =
            draft.GetProjectedValues(
                resourceTarget);

        var allocation =
            ProtectionSharedCapacityAllocator.Allocate(
    projectedResource.Current,
    requestedResourceUnits);

        var items =
            new ProtectionResourceRouteBatchItemResult[
                copiedBindings.Length];

        var routings =
            new ProtectionShortfallRoutingResult[
                copiedBindings.Length];

        // Allocation is completely decided before the first
        // concrete resource loss is staged.
        for (var index = 0;
             index < copiedBindings.Length;
             index++)
        {
            var binding =
                copiedBindings[index];

            var financingPlan =
                financingPlans[index];

            var allocatedResourceUnits =
                allocation.AllocatedCapacities[index];

            var allocatedRequest =
                new ResourceLossRequest(
                    financingPlan.ResourceId,
                    allocatedResourceUnits,
                    financingPlan.ResourceRequest.Provenance);

            var preview =
                draft.StageLoss(
                    financingPlan.ResourceTarget,
                    allocatedRequest);

            // Resolve against the ORIGINAL protection claim,
            // not against the smaller allocated resource request.
            var financingResult =
                financingPlan.Financing.Resolve(
                    preview.Result.ActualLoss);

            var shortfallRouting =
                ProtectionShortfallRouter.Route(
                    financingResult);

            routings[index] =
                shortfallRouting;

            items[index] =
                new ProtectionResourceRouteBatchItemResult(
                    binding,
                    financingPlan.Financing,
                    preview,
                    financingResult,
                    shortfallRouting);
        }

        var updatedState =
            state;

        // These transitions are materialization of already
        // resolved simultaneous claims. Their loop order is
        // not a capacity priority.
        for (var index = 0;
             index < copiedBindings.Length;
             index++)
        {
            updatedState =
                updatedState.Apply(
                    copiedBindings[index].Assignment,
                    routings[index]);
        }

        updatedState.BindResourceTransactionDraft(
            draft);

        return new ProtectionResourceRouteBatchExecutionResult(
            state,
            updatedState,
            items);
    }

    private static bool SharesConcreteResourceSource(
        ProtectionResourceRouteBinding first,
        ProtectionResourceRouteBinding second)
    {
        return
            first.ResourceOwnerEntityId ==
            second.ResourceOwnerEntityId &&
            first.ResourceId ==
            second.ResourceId &&
            ReferenceEquals(
                first.ResourceTarget.ResourceRegistry,
                second.ResourceTarget.ResourceRegistry) &&
            ReferenceEquals(
                first.ResourceTarget.State,
                second.ResourceTarget.State);
    }

    private static ProtectionAssignmentLaneState? FindLane(
        ProtectionAssignmentRoutingState state,
        ProtectionAssignmentResult assignment)
    {
        foreach (var lane in state.Lanes)
        {
            if (ReferenceEquals(
                    lane.Assignment,
                    assignment))
            {
                return lane;
            }
        }

        return null;
    }
}