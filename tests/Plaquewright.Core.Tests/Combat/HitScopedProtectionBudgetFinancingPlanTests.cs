using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetFinancingPlanTests
{
    [Fact]
    public void FullBudgetFinancing_CoversAllAssignedDamage()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var plan =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Reserve();

        Assert.Equal(
            40d,
            result.AssignedDamage);

        Assert.Equal(
            80d,
            result.RequestedBudgetUnits);

        Assert.Equal(
            80d,
            result.ReservedBudgetUnits);

        Assert.Equal(
            0d,
            result.BudgetUnitShortfall);

        Assert.Equal(
            40d,
            result.FinancedDamage);

        Assert.Equal(
            0d,
            result.UnfinancedDamage);

        Assert.Equal(
            20d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void PartialBudgetFinancing_PreservesUnitSeparation()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var plan =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Reserve();

        Assert.Equal(
            80d,
            result.RequestedBudgetUnits);

        Assert.Equal(
            50d,
            result.ReservedBudgetUnits);

        Assert.Equal(
            30d,
            result.BudgetUnitShortfall);

        Assert.Equal(
            25d,
            result.FinancedDamage);

        Assert.Equal(
            15d,
            result.UnfinancedDamage);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            result.ShortfallPolicy);
    }

    [Fact]
    public void ReservationAccounting_IsCompleteInBothUnitDomains()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 50d);

        var result =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        Assert.Equal(
            result.RequestedBudgetUnits,
            result.ReservedBudgetUnits +
            result.BudgetUnitShortfall);

        Assert.Equal(
            result.AssignedDamage,
            result.FinancedDamage +
            result.UnfinancedDamage);
    }

    [Fact]
    public void SharedBudget_IsConsumedAcrossMultipleFinancingPlans()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var first =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 30d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting)
            .Reserve();

        Assert.Equal(
            60d,
            first.ReservedBudgetUnits);

        Assert.Equal(
            30d,
            first.FinancedDamage);

        Assert.Equal(
            40d,
            budget.RemainingCapacity);

        var second =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 30d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        // Second path does not get a fresh 100-unit budget.
        Assert.Equal(
            60d,
            second.RequestedBudgetUnits);

        Assert.Equal(
            40d,
            second.ReservedBudgetUnits);

        Assert.Equal(
            20d,
            second.BudgetUnitShortfall);

        Assert.Equal(
            20d,
            second.FinancedDamage);

        Assert.Equal(
            10d,
            second.UnfinancedDamage);

        Assert.Equal(
            100d,
            budget.ReservedCapacity);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void DifferentRatesAgainstSharedBudget_UseSameRemainingCapacity()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var first =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 25d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting)
            .Reserve();

        // 25 damage costs 50 units.
        Assert.Equal(
            50d,
            first.ReservedBudgetUnits);

        Assert.Equal(
            50d,
            budget.RemainingCapacity);

        var second =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 40d,
                budgetUnitsPerDamage: 5d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        // Requests 200 units, but only 50 remain.
        Assert.Equal(
            200d,
            second.RequestedBudgetUnits);

        Assert.Equal(
            50d,
            second.ReservedBudgetUnits);

        Assert.Equal(
            150d,
            second.BudgetUnitShortfall);

        // 50 budget units / 5 per damage = 10 damage.
        Assert.Equal(
            10d,
            second.FinancedDamage);

        Assert.Equal(
            30d,
            second.UnfinancedDamage);
    }

    [Fact]
    public void ZeroAssignedDamage_DoesNotConsumeBudget()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var result =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 0d,
                budgetUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        Assert.Equal(
            0d,
            result.RequestedBudgetUnits);

        Assert.Equal(
            0d,
            result.ReservedBudgetUnits);

        Assert.Equal(
            0d,
            result.FinancedDamage);

        Assert.Equal(
            0d,
            result.UnfinancedDamage);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void ResultPreservesHitExecutionIdentity()
    {
        var hitExecutionId =
            new HitExecutionId(42UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var result =
            new HitScopedProtectionBudgetFinancingPlan(
                budget,
                assignedDamage: 20d,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Reserve();

        Assert.Equal(
            hitExecutionId,
            result.HitExecutionId);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidAssignedDamage_ThrowsWithoutConsumingBudget(
        double assignedDamage)
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudgetFinancingPlan(
                    budget,
                    assignedDamage,
                    budgetUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidBudgetUnitsPerDamage_ThrowsWithoutConsumingBudget(
        double rate)
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudgetFinancingPlan(
                    budget,
                    assignedDamage: 10d,
                    budgetUnitsPerDamage: rate,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void RequestedBudgetOverflow_ThrowsWithoutConsumingBudget()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        Assert.Throws<OverflowException>(
            () =>
                new HitScopedProtectionBudgetFinancingPlan(
                    budget,
                    assignedDamage: double.MaxValue,
                    budgetUnitsPerDamage: 2d,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void UnknownShortfallPolicy_ThrowsWithoutConsumingBudget()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var invalidPolicy =
            (ProtectionFinancingShortfallPolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudgetFinancingPlan(
                    budget,
                    assignedDamage: 10d,
                    budgetUnitsPerDamage: 1d,
                    invalidPolicy));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);
    }
}