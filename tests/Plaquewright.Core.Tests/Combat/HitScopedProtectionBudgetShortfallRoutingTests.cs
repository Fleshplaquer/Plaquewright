using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetShortfallRoutingTests
{
    [Fact]
    public void SpillBack_RoutesUnfinancedBudgetProtectedDamageToPrimaryPath()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var financingResult =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        Assert.Equal(
            25d,
            financingResult.FinancedDamage);

        Assert.Equal(
            15d,
            financingResult.UnfinancedDamage);

        Assert.Equal(
            15d,
            routing.UnfinancedDamage);

        Assert.Equal(
            15d,
            routing.SpillBackDamage);

        Assert.Equal(
            0d,
            routing.ContinueRoutingDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            routing.Policy);
    }

    [Fact]
    public void ContinueRouting_ForwardsUnfinancedDamageToNextRoute()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var financingResult =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting)
            .Reserve();

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        Assert.Equal(
            25d,
            financingResult.FinancedDamage);

        Assert.Equal(
            15d,
            financingResult.UnfinancedDamage);

        Assert.Equal(
            0d,
            routing.SpillBackDamage);

        Assert.Equal(
            15d,
            routing.ContinueRoutingDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.ContinueRouting,
            routing.Policy);
    }

    [Fact]
    public void BudgetShortfallRouting_UsesDamageUnits_NotBudgetUnits()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var financingResult =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        // Budget domain:
        // requested 80
        // reserved 50
        // shortfall 30.
        Assert.Equal(
            80d,
            financingResult.RequestedBudgetUnits);

        Assert.Equal(
            50d,
            financingResult.ReservedBudgetUnits);

        Assert.Equal(
            30d,
            financingResult.BudgetUnitShortfall);

        // Damage domain:
        // 50 budget / 2 = 25 financed damage
        // 40 - 25 = 15 unfinanced damage.
        Assert.Equal(
            25d,
            financingResult.FinancedDamage);

        Assert.Equal(
            15d,
            routing.SpillBackDamage);

        Assert.NotEqual(
            financingResult.BudgetUnitShortfall,
            routing.SpillBackDamage);
    }

    [Fact]
    public void HitBudgetRoutingResult_CanBeAppliedToExistingAssignmentLane()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            40d,
            state.Lanes[0].ContinueRoutingDamage);

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var financingResult =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        state =
            state.Apply(
                assignment,
                routing);

        // 40 assigned.
        //
        // 50 budget units at 2 / damage
        // finance 25 damage.
        //
        // 15 spill back.
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

        Assert.Equal(
            0d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void HitBudgetRoute_CanContinueIntoResourceBackedRoute()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var budgetFinancing =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting)
            .Reserve();

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    budgetFinancing));

        // 40 entered budget protection.
        // Budget finances 25.
        // 15 continues.
        Assert.Equal(
            15d,
            state.Lanes[0].ContinueRoutingDamage);

        Assert.Equal(
            25d,
            state.Lanes[0].FinancedDamage);

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        // Simulate the next generic protection route here
        // without binding it to a resource yet.
        var nextFinancing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var nextResult =
            nextFinancing.Resolve(
                actualResourceUnitsSpent: 10d);

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    nextResult));

        // Next route finances 10 of the remaining 15.
        // Final 5 spills back.
        Assert.Equal(
            65d,
            state.PrimaryPathDamage);

        Assert.Equal(
            35d,
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
    public void SharedHitBudgetAcrossParallelLanes_PreservesGlobalAccounting()
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

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 40d);

        var firstAssignment =
            resolution.Assignments[0];

        var firstFinancing =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage:
                    state.Lanes[0].ContinueRoutingDamage,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    firstFinancing));

        // Lane A consumes 30 of the shared 40.
        Assert.Equal(
            10d,
            budget.RemainingCapacity);

        Assert.Equal(
            30d,
            firstFinancing.FinancedDamage);

        var secondAssignment =
            resolution.Assignments[1];

        var secondFinancing =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage:
                    state.Lanes[1].ContinueRoutingDamage,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        state =
            state.Apply(
                secondAssignment,
                ProtectionShortfallRouter.Route(
                    secondFinancing));

        // Lane B receives only the remaining 10 capacity.
        Assert.Equal(
            10d,
            secondFinancing.FinancedDamage);

        Assert.Equal(
            10d,
            secondFinancing.UnfinancedDamage);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        // Base primary = 50
        // Lane A financed = 30
        // Lane B financed = 10
        // Lane B spillback = 10
        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            40d,
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
}