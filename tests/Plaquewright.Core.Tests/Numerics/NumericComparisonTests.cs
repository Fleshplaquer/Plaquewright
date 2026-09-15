using Plaquewright.Core.Numerics;

namespace Plaquewright.Core.Tests.Numerics;

public sealed class NumericComparisonTests
{
    [Fact]
    public void AreEquivalent_AcceptsOrdinaryFloatingPointReconstructionNoise()
    {
        var reconstructed =
            0.2d + 0.7d;

        Assert.NotEqual(
            0.9d,
            reconstructed);

        Assert.True(
            NumericComparison.AreEquivalent(
                reconstructed,
                0.9d,
                scale: 0.9d));
    }

    [Fact]
    public void AreEquivalent_AcceptsOneUlpStyleDifference()
    {
        Assert.True(
            NumericComparison.AreEquivalent(
                7.000000000000001d,
                7d,
                scale: 7d));
    }

    [Fact]
    public void AreEquivalent_RejectsMeaningfulDifference()
    {
        Assert.False(
            NumericComparison.AreEquivalent(
                0.89d,
                0.9d,
                scale: 0.9d));
    }

    [Fact]
    public void AreEquivalent_UsesExplicitAccountingScaleForNearZeroResidual()
    {
        var residual =
            0.1d +
            0.2d -
            0.3d;

        Assert.NotEqual(
            0d,
            residual);

        Assert.True(
            NumericComparison.AreEquivalent(
                residual,
                0d,
                scale: 0.3d));
    }

    [Fact]
    public void AreEquivalent_WithNonFiniteValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                NumericComparison.AreEquivalent(
                    double.NaN,
                    1d,
                    scale: 1d));
    }

    [Fact]
    public void AreEquivalent_WithNegativeScale_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                NumericComparison.AreEquivalent(
                    1d,
                    1d,
                    scale: -1d));
    }
}