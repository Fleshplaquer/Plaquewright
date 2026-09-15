using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionFinancingPlanTests
{
    [Fact]
    public void FullFinancing_CoversAllAssignedDamage()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Resolve(
                actualResourceUnitsSpent: 60d);

        Assert.Equal(
            60d,
            result.AssignedDamage);

        Assert.Equal(
            60d,
            result.RequestedResourceUnits);

        Assert.Equal(
            60d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            0d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            60d,
            result.FinancedDamage);

        Assert.Equal(
            0d,
            result.UnfinancedDamage);
    }

    [Fact]
    public void PartialFinancing_WithOneToOneRate_LeavesDamageShortfall()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Resolve(
                actualResourceUnitsSpent: 50d);

        Assert.Equal(
            60d,
            result.AssignedDamage);

        Assert.Equal(
            60d,
            result.RequestedResourceUnits);

        Assert.Equal(
            50d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            10d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            50d,
            result.FinancedDamage);

        Assert.Equal(
            10d,
            result.UnfinancedDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            result.ShortfallPolicy);
    }

    [Fact]
    public void TwoResourceUnitsPerDamage_PreservesUnitSeparation()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Resolve(
                actualResourceUnitsSpent: 50d);

        Assert.Equal(
            60d,
            result.AssignedDamage);

        Assert.Equal(
            120d,
            result.RequestedResourceUnits);

        Assert.Equal(
            50d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            70d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            25d,
            result.FinancedDamage);

        Assert.Equal(
            35d,
            result.UnfinancedDamage);
    }

    [Fact]
    public void ContinueRoutingPolicy_IsPreserved()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 100d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var result =
            plan.Resolve(
                actualResourceUnitsSpent: 40d);

        Assert.Equal(
            60d,
            result.UnfinancedDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.ContinueRouting,
            result.ShortfallPolicy);
    }

    [Fact]
    public void ZeroAssignedDamage_IsAllowed()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 0d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var result =
            plan.Resolve(
                actualResourceUnitsSpent: 0d);

        Assert.Equal(
            0d,
            plan.RequestedResourceUnits);

        Assert.Equal(
            0d,
            result.FinancedDamage);

        Assert.Equal(
            0d,
            result.UnfinancedDamage);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidAssignedDamage_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionFinancingPlan(
                    amount,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidResourceUnitsPerDamage_Throws(
        double rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionFinancingPlan(
                    assignedDamage: 10d,
                    resourceUnitsPerDamage: rate,
                    ProtectionFinancingShortfallPolicy.SpillBack));
    }

    [Fact]
    public void UnknownShortfallPolicy_Throws()
    {
        var invalidPolicy =
            (ProtectionFinancingShortfallPolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionFinancingPlan(
                    assignedDamage: 10d,
                    resourceUnitsPerDamage: 1d,
                    invalidPolicy));
    }

    [Fact]
    public void Resolve_WithMoreResourceSpentThanRequested_Throws()
    {
        var plan =
            new ProtectionFinancingPlan(
                assignedDamage: 10d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                plan.Resolve(
                    actualResourceUnitsSpent: 21d));
    }
}