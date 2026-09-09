using Idler.Core.Combat;

namespace Idler.Core.Tests.Combat;

public sealed class ProtectionAssignmentRoutingStateTests
{
    [Fact]
    public void Start_CreatesIndependentLaneForEachAssignment()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.Equal(
            100d,
            state.InitialDamage);

        Assert.Equal(
            50d,
            state.BasePrimaryPathDamage);

        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.Equal(
            50d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        Assert.Equal(
            2,
            state.Lanes.Count);

        Assert.Equal(
            30d,
            state.Lanes[0].AssignedDamage);

        Assert.Equal(
            30d,
            state.Lanes[0].ContinueRoutingDamage);

        Assert.Equal(
            20d,
            state.Lanes[1].AssignedDamage);

        Assert.Equal(
            20d,
            state.Lanes[1].ContinueRoutingDamage);

        Assert.False(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void ApplyingFirstLane_DoesNotConsumeSecondLane()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var firstAssignment =
            resolution.Assignments[0];

        var firstFinancing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    firstAssignment.AssignedDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var firstRouting =
            ProtectionShortfallRouter.Route(
                firstFinancing.Resolve(
                    actualResourceUnitsSpent: 20d));

        var result =
            state.Apply(
                firstAssignment,
                firstRouting);

        // Lane A:
        // 30 assigned
        // 20 financed
        // 10 spillback.
        Assert.Equal(
            20d,
            result.Lanes[0].FinancedDamage);

        Assert.Equal(
            10d,
            result.Lanes[0].SpillBackDamage);

        Assert.Equal(
            0d,
            result.Lanes[0].ContinueRoutingDamage);

        Assert.True(
            result.Lanes[0].IsComplete);

        // Lane B remains completely untouched.
        Assert.Equal(
            0d,
            result.Lanes[1].FinancedDamage);

        Assert.Equal(
            0d,
            result.Lanes[1].SpillBackDamage);

        Assert.Equal(
            20d,
            result.Lanes[1].ContinueRoutingDamage);

        Assert.False(
            result.Lanes[1].IsComplete);

        Assert.Equal(
            60d,
            result.PrimaryPathDamage);

        Assert.Equal(
            20d,
            result.ContinueRoutingDamage);

        Assert.Equal(
            20d,
            result.FinancedDamage);

        Assert.Equal(
            100d,
            result.AccountedDamage);

        // Original state remains immutable.
        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.Equal(
            50d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);
    }

    [Fact]
    public void MultipleLanes_AccumulateSpillBackIntoSharedPrimaryPath()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var firstAssignment =
            resolution.Assignments[0];

        var firstFinancing =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    firstFinancing.Resolve(
                        actualResourceUnitsSpent: 20d)));

        var secondAssignment =
            resolution.Assignments[1];

        var secondFinancing =
            new ProtectionFinancingPlan(
                assignedDamage: 20d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                secondAssignment,
                ProtectionShortfallRouter.Route(
                    secondFinancing.Resolve(
                        actualResourceUnitsSpent: 5d)));

        // Base primary = 50
        // Lane A spillback = 10
        // Lane B spillback = 15
        Assert.Equal(
            75d,
            state.PrimaryPathDamage);

        Assert.Equal(
            25d,
            state.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void ContinueRouting_RemainsInsideItsOwnLane()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var firstAssignment =
            resolution.Assignments[0];

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    financing.Resolve(
                        actualResourceUnitsSpent: 20d)));

        // Lane A forwards 10 to its next route.
        Assert.Equal(
            10d,
            state.Lanes[0].ContinueRoutingDamage);

        // Lane B still independently has its original 20.
        Assert.Equal(
            20d,
            state.Lanes[1].ContinueRoutingDamage);

        Assert.Equal(
            30d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.Equal(
            20d,
            state.FinancedDamage);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void ContinueRoutingLane_CanApplyAnotherOrderedRoute()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var firstAssignment =
            resolution.Assignments[0];

        var firstRoute =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    firstRoute.Resolve(
                        actualResourceUnitsSpent: 20d)));

        Assert.Equal(
            10d,
            state.Lanes[0].ContinueRoutingDamage);

        var secondRoute =
            new ProtectionFinancingPlan(
                assignedDamage: 10d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    secondRoute.Resolve(
                        actualResourceUnitsSpent: 4d)));

        // Lane A:
        // 20 first route
        // 4 second route
        // 6 spillback
        Assert.Equal(
            24d,
            state.Lanes[0].FinancedDamage);

        Assert.Equal(
            6d,
            state.Lanes[0].SpillBackDamage);

        Assert.Equal(
            0d,
            state.Lanes[0].ContinueRoutingDamage);

        Assert.True(
            state.Lanes[0].IsComplete);

        // Lane B is still completely independent.
        Assert.Equal(
            20d,
            state.Lanes[1].ContinueRoutingDamage);

        Assert.Equal(
            56d,
            state.PrimaryPathDamage);

        Assert.Equal(
            24d,
            state.FinancedDamage);

        Assert.Equal(
            20d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void RoutingResultCannotBeAppliedToAssignmentFromDifferentResolution()
    {
        var firstResolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var secondResolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                firstResolution);

        var foreignAssignment =
            secondResolution.Assignments[0];

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var routing =
            ProtectionShortfallRouter.Route(
                financing.Resolve(
                    actualResourceUnitsSpent: 20d));

        Assert.Throws<InvalidOperationException>(
            () =>
                state.Apply(
                    foreignAssignment,
                    routing));

        Assert.Equal(
            100d,
            state.AccountedDamage);

        Assert.Equal(
            30d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);
    }

    [Fact]
    public void CompletedLane_RejectsAdditionalRoute()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        var first =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    first.Resolve(
                        actualResourceUnitsSpent: 20d)));

        Assert.True(
            state.Lanes[0].IsComplete);

        var invalidAdditionalRoute =
            new ProtectionFinancingPlan(
                assignedDamage: 0d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var routing =
            ProtectionShortfallRouter.Route(
                invalidAdditionalRoute.Resolve(
                    actualResourceUnitsSpent: 0d));

        Assert.Throws<InvalidOperationException>(
            () =>
                state.Apply(
                    assignment,
                    routing));
    }

    [Fact]
    public void ZeroAssignments_ProduceCompletedPrimaryOnlyState()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                Array.Empty<ProtectionAssignmentRequest>());

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.Empty(
            state.Lanes);

        Assert.Equal(
            100d,
            state.PrimaryPathDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void ParallelLanes_AlwaysPreserveGlobalDamageAccounting()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        var firstAssignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 30d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 20d)));

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        var secondAssignment =
            resolution.Assignments[1];

        state =
            state.Apply(
                secondAssignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 20d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.SpillBack)
                    .Resolve(
                        actualResourceUnitsSpent: 5d)));

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 10d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.SpillBack)
                    .Resolve(
                        actualResourceUnitsSpent: 4d)));

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            71d,
            state.PrimaryPathDamage);

        Assert.Equal(
            29d,
            state.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);
    }
}