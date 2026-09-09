using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateOperationsValidationTests
{
    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PreviewLoss_WithInvalidRequestedLoss_Throws(
        double value)
    {
        var state =
            CreateState();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceStateOperations.PreviewLoss(
                    state,
                    requestedLoss: value));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PreviewLoss_WithInvalidPreventedLoss_Throws(
        double value)
    {
        var state =
            CreateState();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceStateOperations.PreviewLoss(
                    state,
                    requestedLoss: 100d,
                    preventedLoss: value));
    }

    [Fact]
    public void PreviewLoss_WithPreventionAboveRequestedLoss_Throws()
    {
        var state =
            CreateState();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceStateOperations.PreviewLoss(
                    state,
                    requestedLoss: 100d,
                    preventedLoss: 101d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PreviewRecovery_WithInvalidRequestedRecovery_Throws(
        double value)
    {
        var state =
            CreateState();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceStateOperations.PreviewRecovery(
                    state,
                    requestedRecovery: value));
    }

    [Fact]
    public void PreviewLoss_WithNullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                ResourceStateOperations.PreviewLoss(
                    null!,
                    requestedLoss: 10d));
    }

    [Fact]
    public void PreviewRecovery_WithNullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                ResourceStateOperations.PreviewRecovery(
                    null!,
                    requestedRecovery: 10d));
    }

    private static ResourceState CreateState()
    {
        return new ResourceState(
            new ResourceId(1),
            current: 50d,
            maximum: 100d);
    }
}