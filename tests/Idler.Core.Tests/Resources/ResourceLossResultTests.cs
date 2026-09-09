using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceLossResultTests
{
    [Fact]
    public void Constructor_PreservesQuantitiesAndComputesShortfall()
    {
        var result =
            new ResourceLossResult(
                new ResourceId(1),
                requestedLoss: 100d,
                preventedLoss: 30d,
                actualLoss: 50d);

        Assert.Equal(
            new ResourceId(1),
            result.ResourceId);

        Assert.Equal(
            100d,
            result.RequestedLoss);

        Assert.Equal(
            30d,
            result.PreventedLoss);

        Assert.Equal(
            50d,
            result.ActualLoss);

        Assert.Equal(
            20d,
            result.Shortfall);
    }

    [Fact]
    public void FullyAppliedLoss_HasNoPreventionOrShortfall()
    {
        var result =
            new ResourceLossResult(
                new ResourceId(1),
                requestedLoss: 100d,
                preventedLoss: 0d,
                actualLoss: 100d);

        Assert.Equal(0d, result.PreventedLoss);
        Assert.Equal(0d, result.Shortfall);
    }

    [Fact]
    public void FullyPreventedLoss_HasNoActualLossOrShortfall()
    {
        var result =
            new ResourceLossResult(
                new ResourceId(1),
                requestedLoss: 100d,
                preventedLoss: 100d,
                actualLoss: 0d);

        Assert.Equal(100d, result.PreventedLoss);
        Assert.Equal(0d, result.ActualLoss);
        Assert.Equal(0d, result.Shortfall);
    }

    [Fact]
    public void CompletelyUnavailableLoss_BecomesShortfall()
    {
        var result =
            new ResourceLossResult(
                new ResourceId(1),
                requestedLoss: 100d,
                preventedLoss: 0d,
                actualLoss: 0d);

        Assert.Equal(100d, result.Shortfall);
    }

    [Fact]
    public void ZeroLoss_IsValid()
    {
        var result =
            new ResourceLossResult(
                new ResourceId(1),
                requestedLoss: 0d,
                preventedLoss: 0d,
                actualLoss: 0d);

        Assert.Equal(0d, result.RequestedLoss);
        Assert.Equal(0d, result.PreventedLoss);
        Assert.Equal(0d, result.ActualLoss);
        Assert.Equal(0d, result.Shortfall);
    }

    [Fact]
    public void PreventedLossAboveRequestedLoss_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceLossResult(
                    new ResourceId(1),
                    requestedLoss: 100d,
                    preventedLoss: 101d,
                    actualLoss: 0d));
    }

    [Fact]
    public void ActualLossAboveRemainingLoss_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceLossResult(
                    new ResourceId(1),
                    requestedLoss: 100d,
                    preventedLoss: 30d,
                    actualLoss: 71d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1d)]
    public void InvalidRequestedLoss_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceLossResult(
                    new ResourceId(1),
                    requestedLoss: value,
                    preventedLoss: 0d,
                    actualLoss: 0d));
    }

    [Fact]
    public void InvalidResourceId_Throws()
    {
        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceLossResult(
                    id,
                    requestedLoss: 0d,
                    preventedLoss: 0d,
                    actualLoss: 0d));
    }
}