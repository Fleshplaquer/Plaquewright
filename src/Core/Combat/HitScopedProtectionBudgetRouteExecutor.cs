namespace Plaquewright.Core.Combat;

internal static class HitScopedProtectionBudgetRouteExecutor
{
    public static HitScopedProtectionBudgetRouteExecutionResult Execute(
        ProtectionAssignmentRoutingState state,
        HitScopedProtectionBudgetRouteBinding binding)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            binding);

        state.ValidateCurrentForTransition();

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

        if (!state.RelatedHitExecutionId.HasValue)
        {
            throw new InvalidOperationException(
                "Hit-scoped protection budget routes require a hit-based protection routing state.");
        }

        if (state.RelatedHitExecutionId.Value !=
            binding.HitExecutionId)
        {
            throw new InvalidOperationException(
                "Hit-scoped protection budget belongs to a different hit execution than the protection routing state.");
        }

        // Chain.Apply increments this counter.
        // Validate before consuming transient budget capacity.
        if (lane.Chain.AppliedRouteCount ==
            int.MaxValue)
        {
            throw new InvalidOperationException(
                "Protection assignment lane cannot accept another route.");
        }

        var financingPlan =
            binding.CreateFinancingPlan(
                lane);

        var financingResult =
            financingPlan.ReservePending(
                out var reservationLease);

        try
        {
            var shortfallRouting =
                ProtectionShortfallRouter.Route(
                    financingResult);

            var updatedState =
                state.Apply(
                    binding.Assignment,
                    shortfallRouting);

            var executionResult =
                new HitScopedProtectionBudgetRouteExecutionResult(
                    state,
                    updatedState,
                    binding,
                    financingResult,
                    shortfallRouting);

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