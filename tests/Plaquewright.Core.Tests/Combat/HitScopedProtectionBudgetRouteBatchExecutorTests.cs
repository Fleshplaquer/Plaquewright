using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetRouteBatchExecutorTests
{
    [Fact]
    public void SharedBudgetClaims_AreAllocatedProportionally()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

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
            StartHitBasedRouting(
                resolution,
                hitExecutionId);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 40d);

        var execution =
            HitScopedProtectionBudgetRouteBatchExecutor.Execute(
                state,
                [
                    new HitScopedProtectionBudgetRouteBinding(
                        resolution.Assignments[0],
                        budget,
                        budgetUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.SpillBack),

                    new HitScopedProtectionBudgetRouteBinding(
                        resolution.Assignments[1],
                        budget,
                        budgetUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.SpillBack)
                ]);

        Assert.Equal(
            2,
            execution.Items.Count);

        var first =
            execution.Items[0];

        var second =
            execution.Items[1];

        // 30 / 50 of shared 40 = 24.
        Assert.Equal(
            30d,
            first.RequestedBudgetUnits);

        Assert.Equal(
            24d,
            first.AllocatedBudgetUnits);

        Assert.Equal(
            6d,
            first.BudgetUnitShortfall);

        Assert.Equal(
            24d,
            first.FinancedDamage);

        Assert.Equal(
            6d,
            first.SpillBackDamage);

        // 20 / 50 of shared 40 = 16.
        Assert.Equal(
            20d,
            second.RequestedBudgetUnits);

        Assert.Equal(
            16d,
            second.AllocatedBudgetUnits);

        Assert.Equal(
            4d,
            second.BudgetUnitShortfall);

        Assert.Equal(
            16d,
            second.FinancedDamage);

        Assert.Equal(
            4d,
            second.SpillBackDamage);

        Assert.Equal(
            40d,
            budget.ReservedCapacity);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        Assert.Equal(
            60d,
            execution.UpdatedState.PrimaryPathDamage);

        Assert.Equal(
            40d,
            execution.UpdatedState.FinancedDamage);

        Assert.Equal(
            0d,
            execution.UpdatedState.ContinueRoutingDamage);

        Assert.True(
            execution.UpdatedState.IsComplete);

        Assert.Equal(
            100d,
            execution.UpdatedState.AccountedDamage);
    }

    [Fact]
    public void BindingOrder_DoesNotCreateSharedBudgetPriority()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

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
            StartHitBasedRouting(
                resolution,
                hitExecutionId);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 40d);

        var thirtyClaim =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var twentyClaim =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[1],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var execution =
            HitScopedProtectionBudgetRouteBatchExecutor.Execute(
                state,
                [
                    twentyClaim,
                    thirtyClaim
                ]);

        Assert.Same(
            twentyClaim,
            execution.Items[0].Binding);

        Assert.Equal(
            16d,
            execution.Items[0].AllocatedBudgetUnits);

        Assert.Same(
            thirtyClaim,
            execution.Items[1].Binding);

        Assert.Equal(
            24d,
            execution.Items[1].AllocatedBudgetUnits);

        Assert.Equal(
            40d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void SeparateBudgetInstancesForSameHit_AreRejectedBeforeReservation()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

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
            StartHitBasedRouting(
                resolution,
                hitExecutionId);

        var firstBudget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 40d);

        var secondBudget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 40d);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteBatchExecutor.Execute(
                    state,
                    [
                        new HitScopedProtectionBudgetRouteBinding(
                            resolution.Assignments[0],
                            firstBudget,
                            budgetUnitsPerDamage: 1d,
                            ProtectionFinancingShortfallPolicy.SpillBack),

                        new HitScopedProtectionBudgetRouteBinding(
                            resolution.Assignments[1],
                            secondBudget,
                            budgetUnitsPerDamage: 1d,
                            ProtectionFinancingShortfallPolicy.SpillBack)
                    ]));

        Assert.Equal(
            0d,
            firstBudget.ReservedCapacity);

        Assert.Equal(
            0d,
            secondBudget.ReservedCapacity);

        Assert.Equal(
            40d,
            firstBudget.RemainingCapacity);

        Assert.Equal(
            40d,
            secondBudget.RemainingCapacity);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    private static ProtectionAssignmentRoutingState StartHitBasedRouting(
        ProtectionAssignmentResolution resolution,
        HitExecutionId hitExecutionId)
    {
        var damageTarget =
            new DamageTargetContext(
                new DamageExecutionId(1UL),
                new EntityId(2UL),
                hitExecutionId);

        return ProtectionAssignmentRoutingStarter.Start(
            resolution,
            damageTarget);
    }
}