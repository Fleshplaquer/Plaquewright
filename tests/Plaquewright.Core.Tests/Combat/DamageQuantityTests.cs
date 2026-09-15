using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageQuantityTests
{
    [Fact]
    public void PostMitigationDamage_PreservesValidAmount()
    {
        var value =
            new PostMitigationDamage(
                10d);

        Assert.Equal(
            10d,
            value.Amount);
    }

    [Fact]
    public void PostTakenScalingDamage_PreservesValidAmount()
    {
        var value =
            new PostTakenScalingDamage(
                20d);

        Assert.Equal(
            20d,
            value.Amount);
    }

    [Fact]
    public void DamageTaken_PreservesValidAmount()
    {
        var value =
            new DamageTaken(
                30d);

        Assert.Equal(
            30d,
            value.Amount);
    }

    [Fact]
    public void ActualResourceLoss_PreservesValidAmount()
    {
        var value =
            new ActualResourceLoss(
                40d);

        Assert.Equal(
            40d,
            value.Amount);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PostMitigationDamage_RejectsInvalidAmounts(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new PostMitigationDamage(
                    amount));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PostTakenScalingDamage_RejectsInvalidAmounts(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new PostTakenScalingDamage(
                    amount));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void DamageTaken_RejectsInvalidAmounts(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new DamageTaken(
                    amount));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ActualResourceLoss_RejectsInvalidAmounts(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ActualResourceLoss(
                    amount));
    }

    [Fact]
    public void AllQuantityTypes_NormalizeNegativeZero()
    {
        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0d),
            BitConverter.DoubleToInt64Bits(
                new PostMitigationDamage(-0d).Amount));

        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0d),
            BitConverter.DoubleToInt64Bits(
                new PostTakenScalingDamage(-0d).Amount));

        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0d),
            BitConverter.DoubleToInt64Bits(
                new DamageTaken(-0d).Amount));

        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0d),
            BitConverter.DoubleToInt64Bits(
                new ActualResourceLoss(-0d).Amount));
    }

    [Fact]
    public void QuantityTypes_RemainStronglySeparated()
    {
        var incoming =
            new IncomingDamage(
                10d);

        var postMitigation =
            new PostMitigationDamage(
                10d);

        var postTakenScaling =
            new PostTakenScalingDamage(
                10d);

        var damageTaken =
            new DamageTaken(
                10d);

        var actualResourceLoss =
            new ActualResourceLoss(
                10d);

        Assert.Equal(
            10d,
            incoming.Amount);

        Assert.Equal(
            10d,
            postMitigation.Amount);

        Assert.Equal(
            10d,
            postTakenScaling.Amount);

        Assert.Equal(
            10d,
            damageTaken.Amount);

        Assert.Equal(
            10d,
            actualResourceLoss.Amount);
    }
}