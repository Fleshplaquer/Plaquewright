using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageTakenResolverTests
{
    [Fact]
    public void CompletedProtectionRouting_ResolvesPrimaryPathDamage()
    {
        var postTakenScaling =
            new PostTakenScalingDamage(
                100d);

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                postTakenScaling.Amount,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.6d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Resolve(
                actualResourceUnitsSpent: 60d);

        state =
            state.Apply(
                resolution.Assignments[0],
                ProtectionShortfallRouter.Route(
                    financing));

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            40d,
            state.PrimaryPathDamage);

        Assert.Equal(
            60d,
            state.FinancedDamage);

        var damageTaken =
            DamageTakenResolver.Resolve(
                postTakenScaling,
                state);

        Assert.Equal(
            40d,
            damageTaken.Amount);
    }

    [Fact]
    public void IncompleteProtectionRouting_IsRejectedUntilFinalized()
    {
        var postTakenScaling =
            new PostTakenScalingDamage(
                100d);

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                postTakenScaling.Amount,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.6d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.False(
            state.IsComplete);

        Assert.Throws<InvalidOperationException>(
            () =>
                DamageTakenResolver.Resolve(
                    postTakenScaling,
                    state));

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.True(
            finalized.IsComplete);

        // The unresolved 60 protection damage was explicitly
        // returned to the primary path.
        Assert.Equal(
            100d,
            finalized.PrimaryPathDamage);

        var damageTaken =
            DamageTakenResolver.Resolve(
                postTakenScaling,
                finalized);

        Assert.Equal(
            100d,
            damageTaken.Amount);
    }

    [Fact]
    public void RoutingForDifferentDamageAmount_IsRejected()
    {
        var postTakenScaling =
            new PostTakenScalingDamage(
                100d);

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 90d,
                []);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.True(
            state.IsComplete);

        Assert.Throws<InvalidOperationException>(
            () =>
                DamageTakenResolver.Resolve(
                    postTakenScaling,
                    state));
    }

    [Fact]
    public void NoProtectionAssignments_PreserveAllDamageTaken()
    {
        var postTakenScaling =
            new PostTakenScalingDamage(
                100d);

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                postTakenScaling.Amount,
                []);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.True(
            state.IsComplete);

        var damageTaken =
            DamageTakenResolver.Resolve(
                postTakenScaling,
                state);

        Assert.Equal(
            100d,
            damageTaken.Amount);
    }
}