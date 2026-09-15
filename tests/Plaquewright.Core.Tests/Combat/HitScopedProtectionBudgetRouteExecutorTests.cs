using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetRouteExecutorTests
{
    [Fact]
    public void Execute_WithSpillBack_ConsumesBudgetAndUpdatesLane()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var execution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                binding);

        Assert.Same(
            state,
            execution.PreviousState);

        Assert.Same(
            binding,
            execution.Binding);

        Assert.Equal(
            new HitExecutionId(7UL),
            execution.HitExecutionId);

        // 40 damage * 2 = 80 requested budget.
        Assert.Equal(
            80d,
            execution.FinancingResult.RequestedBudgetUnits);

        Assert.Equal(
            50d,
            execution.FinancingResult.ReservedBudgetUnits);

        Assert.Equal(
            30d,
            execution.FinancingResult.BudgetUnitShortfall);

        Assert.Equal(
            25d,
            execution.FinancedDamage);

        Assert.Equal(
            15d,
            execution.UnfinancedDamage);

        Assert.Equal(
            15d,
            execution.SpillBackDamage);

        Assert.Equal(
            0d,
            execution.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        var updated =
            execution.UpdatedState;

        Assert.Equal(
            75d,
            updated.PrimaryPathDamage);

        Assert.Equal(
            25d,
            updated.FinancedDamage);

        Assert.Equal(
            0d,
            updated.ContinueRoutingDamage);

        Assert.True(
            updated.IsComplete);

        Assert.Equal(
            100d,
            updated.AccountedDamage);

        // Immutable previous routing state.
        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            40d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);
    }

    [Fact]
    public void PreviousRoutingState_CannotReserveBudgetAgain()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                    requestedFraction: 0.4d)
                ]);

        var staleState =
            StartHitBasedRouting(
                resolution,
                new HitExecutionId(7UL));

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var execution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                staleState,
                binding);

        Assert.Equal(
            40d,
            budget.ReservedCapacity);

        Assert.Equal(
            60d,
            budget.RemainingCapacity);

        Assert.True(
            execution.UpdatedState.IsComplete);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteExecutor.Execute(
                    staleState,
                    binding));

        // Stale routing state is rejected before a second
        // budget reservation occurs.
        Assert.Equal(
            40d,
            budget.ReservedCapacity);

        Assert.Equal(
            60d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void ContinueRouting_NextHitBudgetRouteReceivesOnlyRemainingDamage()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var assignment =
            resolution.Assignments[0];

        var firstBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var firstBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                firstBudget,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var firstExecution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                firstBinding);

        state =
            firstExecution.UpdatedState;

        Assert.Equal(
            25d,
            firstExecution.FinancedDamage);

        Assert.Equal(
            15d,
            firstExecution.ContinueRoutingDamage);

        Assert.Equal(
            15d,
            state.Lanes[0].ContinueRoutingDamage);

        var secondBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 10d);

        var secondBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                secondBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondExecution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                secondBinding);

        state =
            secondExecution.UpdatedState;

        // Critical invariant:
        // second route receives 15 remaining damage,
        // not the original 40.
        Assert.Equal(
            15d,
            secondExecution.FinancingResult.AssignedDamage);

        Assert.Equal(
            15d,
            secondExecution.FinancingResult.RequestedBudgetUnits);

        Assert.Equal(
            10d,
            secondExecution.FinancedDamage);

        Assert.Equal(
            5d,
            secondExecution.SpillBackDamage);

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
    public void ParallelLanes_SharingBudget_ConsumeSameHitScopedCapacity()
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
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var sharedBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 40d);

        var firstBinding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                sharedBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var firstExecution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                firstBinding);

        state =
            firstExecution.UpdatedState;

        Assert.Equal(
            30d,
            firstExecution.FinancedDamage);

        Assert.Equal(
            10d,
            sharedBudget.RemainingCapacity);

        var secondBinding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[1],
                sharedBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondExecution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                secondBinding);

        state =
            secondExecution.UpdatedState;

        // Lane B does not receive a fresh capacity of 40.
        Assert.Equal(
            20d,
            secondExecution.FinancingResult.RequestedBudgetUnits);

        Assert.Equal(
            10d,
            secondExecution.FinancingResult.ReservedBudgetUnits);

        Assert.Equal(
            10d,
            secondExecution.FinancedDamage);

        Assert.Equal(
            10d,
            secondExecution.SpillBackDamage);

        Assert.Equal(
            0d,
            sharedBudget.RemainingCapacity);

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

    [Fact]
    public void ForeignAssignmentBinding_IsRejectedBeforeBudgetReservation()
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
    StartHitBasedRouting(
        firstResolution,
        new HitExecutionId(7UL));

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var foreignBinding =
            new HitScopedProtectionBudgetRouteBinding(
                secondResolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteExecutor.Execute(
                    state,
                    foreignBinding));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void CompletedLane_IsRejectedWithoutAdditionalBudgetReservation()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var state =
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var firstBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 30d);

        var assignment =
            resolution.Assignments[0];

        var firstBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                firstBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                firstBinding)
            .UpdatedState;

        Assert.True(
            state.Lanes[0].IsComplete);

        var secondBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var secondBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                secondBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteExecutor.Execute(
                    state,
                    secondBinding));

        Assert.Equal(
            0d,
            secondBudget.ReservedCapacity);

        Assert.Equal(
            100d,
            secondBudget.RemainingCapacity);
    }

    [Fact]
    public void BindingCannotExecuteAgainstDifferentAssignmentLane()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.25d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.25d)
                ]);

        var state =
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                binding.CreateFinancingPlan(
                    state.Lanes[1]));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void SeparateBudgetInstancesForSameHit_RemainIndependent()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
    StartHitBasedRouting(
        resolution,
        new HitExecutionId(7UL));

        var assignment =
            resolution.Assignments[0];

        var firstBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 20d);

        var firstBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                firstBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        state =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                firstBinding)
            .UpdatedState;

        Assert.Equal(
            0d,
            firstBudget.RemainingCapacity);

        Assert.Equal(
            20d,
            state.Lanes[0].ContinueRoutingDamage);

        var secondBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 20d);

        var secondBinding =
            new HitScopedProtectionBudgetRouteBinding(
                assignment,
                secondBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                secondBinding)
            .UpdatedState;

        Assert.Equal(
            0d,
            secondBudget.RemainingCapacity);

        Assert.Equal(
            40d,
            state.FinancedDamage);

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.True(
            state.IsComplete);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidBindingRate_Throws(
        double rate)
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudgetRouteBinding(
                    resolution.Assignments[0],
                    budget,
                    rate,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void UnknownBindingPolicy_ThrowsWithoutBudgetReservation()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var invalidPolicy =
            (ProtectionFinancingShortfallPolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudgetRouteBinding(
                    resolution.Assignments[0],
                    budget,
                    budgetUnitsPerDamage: 1d,
                    invalidPolicy));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
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