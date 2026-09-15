using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal static class ProtectionResourceRouteExecutor
{
    public static ProtectionResourceRouteExecutionResult Stage(
        ResourceTransactionDraft draft,
        ProtectionAssignmentRoutingState state,
        ProtectionResourceRouteBinding binding)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            binding);

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

        // Prevent the only structural failure in Chain.Apply
        // that could otherwise occur after resource staging.
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
            ProtectionResourceTransactionStager.Stage(
                draft,
                financingPlan);

        var shortfallRouting =
            ProtectionShortfallRouter.Route(
                financingResult);

        var updatedState =
            state.Apply(
                binding.Assignment,
                shortfallRouting);

        return new ProtectionResourceRouteExecutionResult(
            state,
            updatedState,
            binding,
            financingResult,
            shortfallRouting);
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