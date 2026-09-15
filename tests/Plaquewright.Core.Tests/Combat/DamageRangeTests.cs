using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageRangeTests
{
    [Fact]
    public void Constructor_WithValidRange_PreservesValues()
    {
        var range = new DamageRange(100d, 150d);

        Assert.Equal(100d, range.Min);
        Assert.Equal(150d, range.Max);
    }

    [Fact]
    public void Average_ReturnsRangeMidpoint()
    {
        var range = new DamageRange(100d, 150d);

        Assert.Equal(125d, range.Average);
    }

    [Fact]
    public void IsCollapsed_WithEqualValues_ReturnsTrue()
    {
        var range = new DamageRange(125d, 125d);

        Assert.True(range.IsCollapsed);
    }

    [Fact]
    public void IsCollapsed_WithDifferentValues_ReturnsFalse()
    {
        var range = new DamageRange(100d, 150d);

        Assert.False(range.IsCollapsed);
    }

    [Fact]
    public void Constructor_WithNegativeMinimum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DamageRange(-1d, 100d));
    }

    [Fact]
    public void Constructor_WithNegativeMaximum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DamageRange(0d, -1d));
    }

    [Fact]
    public void Constructor_WithMinimumGreaterThanMaximum_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new DamageRange(200d, 100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithNonFiniteMinimum_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DamageRange(value, 100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithNonFiniteMaximum_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DamageRange(0d, value));
    }
}