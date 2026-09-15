using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResolutionQuantitiesTests
{
    [Fact]
    public void Create_PreservesEveryDamageStage()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var quantities =
            DamageResolutionQuantities.Create(
                incoming,
                postMitigationAmount: 80d,
                postTakenScalingAmount: 90d,
                damageTakenAmount: 70d);

        Assert.Equal(
            100d,
            quantities.Incoming.Amount);

        Assert.Equal(
            80d,
            quantities.PostMitigation.Amount);

        Assert.Equal(
            90d,
            quantities.PostTakenScaling.Amount);

        Assert.Equal(
            70d,
            quantities.Taken.Amount);
    }

    [Fact]
    public void Create_DoesNotRequireNumericallyMonotonicStages()
    {
        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 125d,
                postTakenScalingAmount: 75d,
                damageTakenAmount: 90d);

        Assert.Equal(
            100d,
            quantities.Incoming.Amount);

        Assert.Equal(
            125d,
            quantities.PostMitigation.Amount);

        Assert.Equal(
            75d,
            quantities.PostTakenScaling.Amount);

        Assert.Equal(
            90d,
            quantities.Taken.Amount);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInvalidPostMitigationAmount_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: amount,
                    postTakenScalingAmount: 80d,
                    damageTakenAmount: 70d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInvalidPostTakenScalingAmount_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 90d,
                    postTakenScalingAmount: amount,
                    damageTakenAmount: 70d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInvalidDamageTakenAmount_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 90d,
                    postTakenScalingAmount: 80d,
                    damageTakenAmount: amount));
    }

    [Fact]
    public void Snapshot_IsIndependentFromLaterResourceLoss()
    {
        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var resourceLoss =
            quantities.Taken.AdvanceToActualResourceLoss(
                25d);

        Assert.Equal(
            60d,
            quantities.Taken.Amount);

        Assert.Equal(
            25d,
            resourceLoss.Amount);
    }
}