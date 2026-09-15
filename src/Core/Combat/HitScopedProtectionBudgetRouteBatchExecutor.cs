namespace Plaquewright.Core.Combat;

internal static class HitScopedProtectionBudgetRouteBatchExecutor
{
    public static HitScopedProtectionBudgetRouteBatchExecutionResult Execute(
        ProtectionAssignmentRoutingState state,
        IReadOnlyList<HitScopedProtectionBudgetRouteBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            bindings);

        if (bindings.Count == 0)
        {
            throw new ArgumentException(
                "A shared hit-scoped protection budget batch requires at least one binding.",
                nameof(bindings));
        }

        state.ValidateCurrentForTransition();

        if (!state.RelatedHitExecutionId.HasValue)
        {
            throw new InvalidOperationException(
                "Hit-scoped protection budget routes require a hit-based protection routing state.");
        }

        var copiedBindings =
            new HitScopedProtectionBudgetRouteBinding[
                bindings.Count];

        var financingPlans =
            new HitScopedProtectionBudgetFinancingPlan[
                bindings.Count];

        var requestedBudgetUnits =
            new double[
                bindings.Count];

        HitScopedProtectionBudget?
            sharedBudget = null;

        for (var index = 0;
             index < bindings.Count;
             index++)
        {
            var binding =
                bindings[index];

            if (binding is null)
            {
                throw new ArgumentException(
                    "Shared hit-scoped protection budget batch cannot contain null bindings.",
                    nameof(bindings));
            }

            copiedBindings[index] =
                binding;

            if (binding.HitExecutionId !=
                state.RelatedHitExecutionId.Value)
            {
                throw new InvalidOperationException(
                    "Hit-scoped protection budget belongs to a different hit execution than the protection routing state.");
            }

            if (sharedBudget is null)
            {
                sharedBudget =
                    binding.Budget;
            }
            else if (!ReferenceEquals(
                         sharedBudget,
                         binding.Budget))
            {
                throw new InvalidOperationException(
                    "All bindings in a shared hit-scoped protection budget batch must use the same concrete budget instance.");
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
                        "A protection assignment cannot appear more than once in the same shared hit-scoped protection budget batch.");
                }
            }

            var lane =
                FindLane(
                    state,
                    binding.Assignment);

            if (lane is null)
            {
                throw new InvalidOperationException(
                    "Hit-scoped protection budget route binding does not belong to this routing state.");
            }

            if (lane.IsComplete)
            {
                throw new InvalidOperationException(
                    "Cannot execute another hit-scoped protection route for a completed assignment lane.");
            }

            if (lane.Chain.AppliedRouteCount ==
                int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Protection assignment lane cannot accept another route.");
            }

            var financingPlan =
                binding.CreateFinancingPlan(
                    lane);

            financingPlans[index] =
                financingPlan;

            requestedBudgetUnits[index] =
                financingPlan.RequestedBudgetUnits;
        }

        var allocation =
            ProtectionSharedCapacityAllocator
                .AllocateProportionally(
                    sharedBudget!.RemainingCapacity,
                    requestedBudgetUnits);

        // Reserve the entire already-decided batch allocation
        // as one pending mutation. This preserves the A05
        // abort contract across the whole batch.
        var reservationLease =
            sharedBudget.ReservePending(
                sharedBudget.HitExecutionId,
                allocation.TotalAllocatedCapacity);

        try
        {
            var financingResults =
                new HitScopedProtectionBudgetFinancingResult[
                    copiedBindings.Length];

            var routings =
                new ProtectionShortfallRoutingResult[
                    copiedBindings.Length];

            var items =
                new HitScopedProtectionBudgetRouteBatchItemResult[
                    copiedBindings.Length];

            // All claims were allocated before the shared
            // budget was consumed. Loop order here is not
            // a capacity priority.
            for (var index = 0;
                 index < copiedBindings.Length;
                 index++)
            {
                var financingResult =
                    financingPlans[index]
                        .ResolveReservedBudgetUnits(
                            allocation
                                .AllocatedCapacities[index]);

                var routing =
                    ProtectionShortfallRouter.Route(
                        financingResult);

                financingResults[index] =
                    financingResult;

                routings[index] =
                    routing;

                items[index] =
                    new HitScopedProtectionBudgetRouteBatchItemResult(
                        copiedBindings[index],
                        financingResult,
                        routing);
            }

            var updatedState =
                state;

            for (var index = 0;
                 index < copiedBindings.Length;
                 index++)
            {
                updatedState =
                    updatedState.Apply(
                        copiedBindings[index].Assignment,
                        routings[index]);
            }

            var executionResult =
                new HitScopedProtectionBudgetRouteBatchExecutionResult(
                    state,
                    updatedState,
                    items);

            reservationLease.Commit();

            return executionResult;
        }
        catch
        {
            if (!reservationLease.IsFinalized)
            {
                reservationLease.Abort();
            }

            throw;
        }
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