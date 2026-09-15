using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class ModifierAccumulatorTests
{
    [Fact]
    public void NewAccumulator_StartsEmpty()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Equal(0d, accumulator.Flat);
        Assert.Equal(0d, accumulator.Increased);
        Assert.Equal(0d, accumulator.Reduced);
        Assert.Equal(0, accumulator.MoreCount);
        Assert.Equal(0, accumulator.LessCount);
    }

    [Fact]
    public void AddFlat_SumsMultipleSources()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddFlat(20d);
        accumulator.AddFlat(30d);
        accumulator.AddFlat(-10d);

        Assert.Equal(
            40d,
            accumulator.Flat);
    }

    [Fact]
    public void AddIncreased_SumsMultipleSources()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddIncreased(0.20d);
        accumulator.AddIncreased(0.30d);

        Assert.Equal(
            0.50d,
            accumulator.Increased,
            precision: 10);
    }

    [Fact]
    public void AddReduced_SumsMultipleSources()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddReduced(0.10d);
        accumulator.AddReduced(0.20d);

        Assert.Equal(
            0.30d,
            accumulator.Reduced,
            precision: 10);
    }

    [Fact]
    public void AddMore_PreservesSeparateMultipliers()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddMore(0.20d);
        accumulator.AddMore(0.50d);

        Assert.Equal(
            2,
            accumulator.MoreCount);

        var result =
            accumulator.Apply(100d);

        Assert.Equal(
            180d,
            result,
            precision: 10);
    }

    [Fact]
    public void AddLess_PreservesSeparateMultipliers()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddLess(0.20d);
        accumulator.AddLess(0.50d);

        Assert.Equal(
            2,
            accumulator.LessCount);

        var result =
            accumulator.Apply(100d);

        Assert.Equal(
            40d,
            result,
            precision: 10);
    }

    [Fact]
    public void Apply_CombinesAllModifierFamilies()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddFlat(20d);

        accumulator.AddIncreased(0.20d);
        accumulator.AddIncreased(0.30d);

        accumulator.AddReduced(0.10d);
        accumulator.AddReduced(0.20d);

        accumulator.AddMore(0.50d);

        accumulator.AddLess(0.25d);

        var result =
            accumulator.Apply(100d);

        Assert.Equal(
            162d,
            result,
            precision: 10);
    }

    [Fact]
    public void IncreasedAndReduced_AreAdditivePools()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddIncreased(0.20d);
        accumulator.AddIncreased(0.30d);

        accumulator.AddReduced(0.10d);
        accumulator.AddReduced(0.20d);

        var result =
            accumulator.Apply(100d);

        Assert.Equal(
            120d,
            result,
            precision: 10);
    }

    [Fact]
    public void SourceOrder_DoesNotChangeResult()
    {
        var first =
            new ModifierAccumulator();

        first.AddFlat(20d);
        first.AddIncreased(0.30d);
        first.AddReduced(0.10d);
        first.AddMore(0.50d);
        first.AddLess(0.25d);

        var second =
            new ModifierAccumulator();

        second.AddLess(0.25d);
        second.AddMore(0.50d);
        second.AddReduced(0.10d);
        second.AddIncreased(0.30d);
        second.AddFlat(20d);

        Assert.Equal(
            first.Apply(100d),
            second.Apply(100d));
    }

    [Fact]
    public void AddFlat_AllowsNegativeValues()
    {
        var accumulator =
            new ModifierAccumulator();

        accumulator.AddFlat(-50d);

        Assert.Equal(
            50d,
            accumulator.Apply(100d));
    }

    [Fact]
    public void AddIncreased_WithNegativeValue_Throws()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddIncreased(-0.20d));
    }

    [Fact]
    public void AddReduced_WithNegativeValue_Throws()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddReduced(-0.20d));
    }

    [Fact]
    public void AddMore_WithNegativeValue_Throws()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddMore(-0.20d));
    }

    [Fact]
    public void AddLess_WithNegativeValue_Throws()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddLess(-0.20d));
    }

    [Fact]
    public void AddLess_AboveOneHundredPercent_Throws()
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddLess(1.01d));
    }

    [Fact]
    public void MultipleSameFamilySources_AreOrderIndependent()
    {
        var first =
            new ModifierAccumulator();

        first.AddFlat(20d);
        first.AddFlat(-10d);
        first.AddFlat(30d);

        first.AddIncreased(0.30d);
        first.AddIncreased(0.10d);
        first.AddIncreased(0.20d);

        first.AddReduced(0.20d);
        first.AddReduced(0.05d);

        var second =
            new ModifierAccumulator();

        second.AddReduced(0.05d);
        second.AddIncreased(0.20d);
        second.AddFlat(30d);
        second.AddIncreased(0.10d);
        second.AddFlat(20d);
        second.AddReduced(0.20d);
        second.AddIncreased(0.30d);
        second.AddFlat(-10d);

        Assert.Equal(
            first.Apply(100d),
            second.Apply(100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AddFlat_WithNonFiniteValue_Throws(
        double value)
    {
        var accumulator =
            new ModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddFlat(value));
    }
}