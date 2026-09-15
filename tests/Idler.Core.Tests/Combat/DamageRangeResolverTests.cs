using Idler.Core.Combat;

namespace Idler.Core.Tests.Combat;

public sealed class DamageRangeResolverTests
{
    [Fact]
    public void Resolve_WithOrderedPositiveCandidates_PreservesRange()
    {
        var range =
            DamageRangeResolver.Resolve(
                100d,
                150d);

        Assert.Equal(
            new DamageRange(100d, 150d),
            range);
    }
    [Fact]
    public void Resolve_WithUnknownPolicyAndOrderedCandidates_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageRangeResolver.Resolve(
                    1d,
                    2d,
                    (RangeResolutionPolicy)999));
    }

    [Fact]
    public void Resolve_WithUnknownPolicyAndInvertedCandidates_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageRangeResolver.Resolve(
                    2d,
                    1d,
                    (RangeResolutionPolicy)999));
    }

    [Fact]
    public void Resolve_WithInvertedCandidates_CollapsesBetween()
    {
        var range =
            DamageRangeResolver.Resolve(
                200d,
                50d);

        Assert.Equal(
            new DamageRange(125d, 125d),
            range);
    }

    [Fact]
    public void Resolve_WithConfirmedVariantB_ResolvesBeforeClamping()
    {
        var range =
            DamageRangeResolver.Resolve(
                10d,
                -10d);

        Assert.Equal(
            new DamageRange(0d, 0d),
            range);
    }

    [Fact]
    public void Resolve_WithNegativeOrderedRange_ClampsAfterResolution()
    {
        var range =
            DamageRangeResolver.Resolve(
                -100d,
                -50d);

        Assert.Equal(
            new DamageRange(0d, 0d),
            range);
    }

    [Fact]
    public void Resolve_WithRangeCrossingZero_ClampsOnlyNegativeSide()
    {
        var range =
            DamageRangeResolver.Resolve(
                -50d,
                100d);

        Assert.Equal(
            new DamageRange(0d, 100d),
            range);
    }

    [Fact]
    public void Resolve_WithInvertedPositiveAndNegativeCandidates_CanRemainPositive()
    {
        var range =
            DamageRangeResolver.Resolve(
                30d,
                -10d);

        Assert.Equal(
            new DamageRange(10d, 10d),
            range);
    }

    [Fact]
    public void Resolve_WithInvertedCandidatesAndNegativeMidpoint_ClampsToZero()
    {
        var range =
            DamageRangeResolver.Resolve(
                10d,
                -30d);

        Assert.Equal(
            new DamageRange(0d, 0d),
            range);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Resolve_WithNonFiniteMinimum_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DamageRangeResolver.Resolve(
                value,
                100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Resolve_WithNonFiniteMaximum_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DamageRangeResolver.Resolve(
                0d,
                value));
    }
}