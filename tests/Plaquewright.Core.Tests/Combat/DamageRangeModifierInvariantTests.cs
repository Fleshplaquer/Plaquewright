using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageRangeModifierInvariantTests
{
    [Fact]
    public void RepresentativeModifierCombinations_AlwaysProduceValidDamageRange()
    {
        var baseRanges = new[]
        {
            new DamageRange(0d, 0d),
            new DamageRange(1d, 1d),
            new DamageRange(10d, 20d),
            new DamageRange(100d, 150d),
            new DamageRange(1000d, 5000d)
        };

        var flatValues = new[]
        {
            -200d,
            -50d,
            0d,
            50d,
            200d
        };

        var increasedValues = new[]
        {
            0d,
            0.25d,
            1d,
            3d
        };

        var reducedValues = new[]
        {
            0d,
            0.25d,
            1d,
            2d
        };

        foreach (var baseRange in baseRanges)
        {
            foreach (var minimumFlat in flatValues)
            {
                foreach (var maximumFlat in flatValues)
                {
                    foreach (var increased in increasedValues)
                    {
                        foreach (var reduced in reducedValues)
                        {
                            var accumulator =
                                new DamageRangeModifierAccumulator();

                            accumulator.AddFlat(
                                minimumFlat,
                                DamageRangeTarget.Minimum);

                            accumulator.AddFlat(
                                maximumFlat,
                                DamageRangeTarget.Maximum);

                            accumulator.AddIncreased(increased);
                            accumulator.AddReduced(reduced);

                            var result =
                                accumulator.Apply(baseRange);

                            Assert.True(
                                double.IsFinite(result.Min));

                            Assert.True(
                                double.IsFinite(result.Max));

                            Assert.True(
                                result.Min >= 0d);

                            Assert.True(
                                result.Max >= 0d);

                            Assert.True(
                                result.Min <= result.Max);
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void ModifierSourcePermutation_DoesNotChangeResult()
    {
        var baseRange =
            new DamageRange(100d, 200d);

        var permutations = new[]
        {
            new[] { 0, 1, 2, 3 },
            new[] { 3, 2, 1, 0 },
            new[] { 1, 3, 0, 2 },
            new[] { 2, 0, 3, 1 }
        };

        DamageRange? expected = null;

        foreach (var permutation in permutations)
        {
            var accumulator =
                new DamageRangeModifierAccumulator();

            foreach (var operation in permutation)
            {
                ApplyOperation(
                    accumulator,
                    operation);
            }

            var result =
                accumulator.Apply(baseRange);

            if (expected is null)
            {
                expected = result;
                continue;
            }

            Assert.Equal(
                expected.Value,
                result);
        }
    }

    [Fact]
    public void SameModifierAppliedToBothBounds_PreservesRelativeOrderingBeforeResolution()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(50d);
        accumulator.AddIncreased(0.50d);
        accumulator.AddMore(0.25d);
        accumulator.AddLess(0.20d);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 200d));

        Assert.True(
            result.Min <= result.Max);

        Assert.False(
            result.IsCollapsed);
    }

    [Fact]
    public void CrossedCandidates_AlwaysCollapseToSingleValue()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddFlat(
            200d,
            DamageRangeTarget.Minimum);

        accumulator.AddFlat(
            -200d,
            DamageRangeTarget.Maximum);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 150d));

        Assert.True(result.IsCollapsed);

        Assert.Equal(
            result.Min,
            result.Max);
    }

    [Fact]
    public void DamageDomainClamp_NeverTurnsNegativeResultIntoHealing()
    {
        var accumulator =
            new DamageRangeModifierAccumulator();

        accumulator.AddReduced(2d);

        var result =
            accumulator.Apply(
                new DamageRange(100d, 200d));

        Assert.Equal(
            new DamageRange(0d, 0d),
            result);
    }

    private static void ApplyOperation(
        DamageRangeModifierAccumulator accumulator,
        int operation)
    {
        switch (operation)
        {
            case 0:
                accumulator.AddFlat(
                    25d,
                    DamageRangeTarget.Minimum);
                break;

            case 1:
                accumulator.AddIncreased(0.30d);
                break;

            case 2:
                accumulator.AddMore(0.50d);
                break;

            case 3:
                accumulator.AddLess(0.20d);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation));
        }
    }
}