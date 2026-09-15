using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

internal static class DamageResolutionTestFactory
{
    public static DamageResolutionQuantities Create(
        IncomingDamage incoming,
        double postMitigationAmount,
        double postTakenScalingAmount,
        double damageTakenAmount)
    {
        var protectionRouting =
            CreateCompletedProtectionRouting(
                postTakenScalingAmount,
                damageTakenAmount);

        return DamageResolutionQuantities.Create(
            incoming,
            postMitigationAmount,
            postTakenScalingAmount,
            protectionRouting);
    }

    public static ProtectionAssignmentRoutingState
        CreateCompletedProtectionRouting(
            double postTakenScalingAmount,
            double damageTakenAmount)
    {
        if (!double.IsFinite(
                postTakenScalingAmount) ||
            postTakenScalingAmount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(postTakenScalingAmount),
                postTakenScalingAmount,
                "Post-taken-scaling damage must be finite and non-negative.");
        }

        if (!double.IsFinite(
                damageTakenAmount) ||
            damageTakenAmount < 0d ||
            damageTakenAmount > postTakenScalingAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damageTakenAmount),
                damageTakenAmount,
                "Damage taken must be finite, non-negative, and cannot exceed post-taken-scaling damage.");
        }

        if (damageTakenAmount ==
            postTakenScalingAmount)
        {
            var noProtectionResolution =
                ProtectionAssignmentResolver.Resolve(
                    postTakenScalingAmount,
                    []);

            return ProtectionAssignmentRoutingStarter.Start(
                noProtectionResolution);
        }

        var protectionResolution =
            ProtectionAssignmentResolver.Resolve(
                postTakenScalingAmount,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 1d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                protectionResolution);

        var protectedDamage =
            postTakenScalingAmount -
            damageTakenAmount;

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Resolve(
                actualResourceUnitsSpent:
                    protectedDamage);

        state =
            state.Apply(
                protectionResolution.Assignments[0],
                ProtectionShortfallRouter.Route(
                    financing));

        if (!state.IsComplete)
        {
            throw new InvalidOperationException(
                "Test protection routing did not complete.");
        }

        return state;
    }
}