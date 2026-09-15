using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class IncomingDamageTests
{
    [Fact]
    public void Constructor_WithPositiveAmount_PreservesValue()
    {
        var damage =
            new IncomingDamage(
                42.5d);

        Assert.Equal(
            42.5d,
            damage.Amount);
    }

    [Fact]
    public void Constructor_WithZero_IsAllowed()
    {
        var damage =
            new IncomingDamage(
                0d);

        Assert.Equal(
            0d,
            damage.Amount);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IncomingDamage(
                    -1d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithNonFiniteAmount_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IncomingDamage(
                    amount));
    }

    [Fact]
    public void Constructor_NormalizesNegativeZero()
    {
        var damage =
            new IncomingDamage(
                -0d);

        Assert.Equal(
            0d,
            damage.Amount);

        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0d),
            BitConverter.DoubleToInt64Bits(
                damage.Amount));
    }

    [Fact]
    public void Equality_UsesAmount()
    {
        var first =
            new IncomingDamage(
                42d);

        var second =
            new IncomingDamage(
                42d);

        var other =
            new IncomingDamage(
                43d);

        Assert.Equal(
            first,
            second);

        Assert.NotEqual(
            first,
            other);
    }

    [Fact]
    public void ToString_ReturnsAmount()
    {
        var damage =
            new IncomingDamage(
                42d);

        Assert.Equal(
            "42",
            damage.ToString());
    }
}