using Idler.Core.Combat;

namespace Idler.Core.Tests.Combat;

public sealed class DamageRangeModifierAccumulatorTests
{
    [Fact]
    public void Apply_WithNoModifiers_PreservesRange()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.Equal(
            new DamageRange(100d, 150d),
            result);
    }

    [Fact]
    public void FlatTargetingBoth_ModifiesBothBounds()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(20d);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.Equal(
            new DamageRange(120d, 170d),
            result);
    }

    [Fact]
    public void MinimumAndMaximum_CanBeModifiedIndependently()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(
            25d,
            DamageRangeTarget.Minimum);

        accumulator.AddFlat(
            50d,
            DamageRangeTarget.Maximum);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.Equal(
            new DamageRange(125d, 200d),
            result);
    }

    [Fact]
    public void IncreasedOnBoth_ScalesBothBounds()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddIncreased(0.50d);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.Equal(
            new DamageRange(150d, 225d),
            result);
    }

    [Fact]
    public void SeparateModifierFamilies_CanCrossRange()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(
            100d,
            DamageRangeTarget.Minimum);

        accumulator.AddFlat(
            -100d,
            DamageRangeTarget.Maximum);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.Equal(
            new DamageRange(125d, 125d),
            result);
    }

    [Fact]
    public void MinimumAndMaximumScaling_ResolveOnlyAfterCalculation()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddIncreased(
            1d,
            DamageRangeTarget.Minimum);

        accumulator.AddLess(
            0.50d,
            DamageRangeTarget.Maximum);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        // Minimum:
        // 100 * 2 = 200
        //
        // Maximum:
        // 150 * 0.5 = 75
        //
        // CollapseBetween:
        // (200 + 75) / 2 = 137.5

        Assert.Equal(
            new DamageRange(137.5d, 137.5d),
            result);
    }

    [Fact]
    public void CollapseOccursBeforeDamageClamp()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(
            -20d,
            DamageRangeTarget.Maximum);

        var result =
            accumulator.Apply(
                new DamageRange(10d, 10d));

        // Candidates:
        // Min = 10
        // Max = -10
        //
        // CollapseBetween:
        // 0
        //
        // Clamp:
        // 0

        Assert.Equal(
            new DamageRange(0d, 0d),
            result);
    }

    [Fact]
    public void IncreasedAndReduced_FromMultipleSources_CombinePerBound()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddIncreased(0.20d);
        accumulator.AddIncreased(0.30d);

        accumulator.AddReduced(0.10d);
        accumulator.AddReduced(0.20d);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 200d));

        Assert.Equal(
            new DamageRange(120d, 240d),
            result);
    }

    [Fact]
    public void SourceOrder_DoesNotChangeResult()
    {
        var first =
            new DamageRangeModifierAccumulator();

        first.AddFlat(
            30d,
            DamageRangeTarget.Minimum);

        first.AddIncreased(0.20d);
        first.AddMore(0.50d);
        first.AddLess(0.25d);

        var second =
            new DamageRangeModifierAccumulator();

        second.AddLess(0.25d);
        second.AddMore(0.50d);
        second.AddIncreased(0.20d);

        second.AddFlat(
            30d,
            DamageRangeTarget.Minimum);

        var baseRange =
            new DamageRange(100d, 150d);

        Assert.Equal(
            first.Apply(baseRange),
            second.Apply(baseRange));
    }

    [Fact]
    public void InvalidTarget_Throws()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => accumulator.AddFlat(
                10d,
                (DamageRangeTarget)999));
    }
}