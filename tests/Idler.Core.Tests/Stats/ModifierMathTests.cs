using Idler.Core.Stats;

namespace Idler.Core.Tests.Stats;

public sealed class ModifierMathTests
{
    [Fact]
    public void Apply_WithOnlyBase_ReturnsBase()
    {
        var result =
            ModifierMath.Apply(100d);

        Assert.Equal(100d, result);
    }

    [Fact]
    public void Apply_AddsFlatBeforeScaling()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                flat: 50d,
                increased: 1d);

        Assert.Equal(
            300d,
            result);
    }

    [Fact]
    public void Apply_IncreasedAndReducedShareAdditivePool()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                increased: 0.50d,
                reduced: 0.20d);

        Assert.Equal(
            130d,
            result);
    }

    [Fact]
    public void MultipleIncreasedModifiers_AreAddedTogether()
    {
        var result = ModifierMath.Apply(
            baseValue: 100d,
            increased: 0.20d + 0.30d);

        Assert.Equal(150d, result);
    }

    [Fact]
    public void Apply_CombinedIncreasedAndReducedPools_UsesNetAdditiveScaling()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                increased: 0.20d + 0.30d,
                reduced: 0.10d + 0.20d);

        Assert.Equal(
            120d,
            result,
            precision: 10);
    }
    [Fact]
    public void Apply_ZeroPercentMoreAndLess_DoNotChangeValue()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                more:
                [
                    0d
                ],
                less:
                [
                    0d
                ]);

        Assert.Equal(
            100d,
            result);
    }

    [Fact]
    public void Apply_OneHundredPercentLess_DominatesOtherPositiveScaling()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                increased: 10d,
                more:
                [
                    5d,
                10d
                ],
                less:
                [
                    1d
                ]);

        Assert.Equal(
            0d,
            result);
    }

    [Fact]
    public void Apply_MultipleMoreModifiersMultiply()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                more:
                [
                    0.20d,
                    0.50d
                ]);

        Assert.Equal(
            180d,
            result,
            precision: 10);
    }

    [Fact]
    public void Apply_MultipleLessModifiersMultiply()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                less:
                [
                    0.20d,
                    0.50d
                ]);

        Assert.Equal(
            40d,
            result,
            precision: 10);
    }

    [Fact]
    public void Apply_UsesFullModifierFormula()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                flat: 20d,
                increased: 0.50d,
                reduced: 0.10d,
                more:
                [
                    0.20d,
                    0.50d
                ],
                less:
                [
                    0.25d
                ]);

        // (100 + 20)
        // × (1 + 0.50 - 0.10)
        // × 1.20
        // × 1.50
        // × 0.75
        //
        // = 226.8

        Assert.Equal(
            226.8d,
            result,
            precision: 10);
    }

    [Fact]
    public void Apply_OneHundredPercentLess_ReturnsZero()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                less:
                [
                    1d
                ]);

        Assert.Equal(
            0d,
            result);
    }

    [Fact]
    public void Apply_ReducedCanPushResultBelowZero()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 100d,
                reduced: 1.50d);

        Assert.Equal(
            -50d,
            result);
    }

    [Fact]
    public void Apply_DoesNotClampGenericResult()
    {
        var result =
            ModifierMath.Apply(
                baseValue: 10d,
                flat: -20d);

        Assert.Equal(
            -10d,
            result);
    }

    [Fact]
    public void Apply_MoreOrderDoesNotChangeResult()
    {
        var first =
            ModifierMath.Apply(
                100d,
                more:
                [
                    0.10d,
                    0.25d,
                    0.50d
                ]);

        var second =
            ModifierMath.Apply(
                100d,
                more:
                [
                    0.50d,
                    0.10d,
                    0.25d
                ]);

        Assert.Equal(
            first,
            second);
    }

    [Fact]
    public void Apply_LessOrderDoesNotChangeResult()
    {
        var first =
            ModifierMath.Apply(
                100d,
                less:
                [
                    0.10d,
                    0.25d,
                    0.50d
                ]);

        var second =
            ModifierMath.Apply(
                100d,
                less:
                [
                    0.50d,
                    0.10d,
                    0.25d
                ]);

        Assert.Equal(
            first,
            second);
    }

    [Fact]
    public void Apply_NegativeIncreased_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(
                100d,
                increased: -0.20d));
    }

    [Fact]
    public void Apply_NegativeReduced_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(
                100d,
                reduced: -0.20d));
    }

    [Fact]
    public void Apply_NegativeMore_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(
                100d,
                more:
                [
                    -0.20d
                ]));
    }

    [Fact]
    public void Apply_NegativeLess_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(
                100d,
                less:
                [
                    -0.20d
                ]));
    }

    [Fact]
    public void Apply_LessAboveOneHundredPercent_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(
                100d,
                less:
                [
                    1.01d
                ]));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Apply_WithNonFiniteBase_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ModifierMath.Apply(value));
    }
}