using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceRecoveryResultTests
{
    [Fact]
    public void Constructor_PreservesQuantitiesAndComputesOverflow()
    {
        var result =
            new ResourceRecoveryResult(
                new ResourceId(1),
                requestedRecovery: 100d,
                actualRecovery: 40d);

        Assert.Equal(
            new ResourceId(1),
            result.ResourceId);

        Assert.Equal(
            100d,
            result.RequestedRecovery);

        Assert.Equal(
            40d,
            result.ActualRecovery);

        Assert.Equal(
            60d,
            result.Overflow);
    }

    [Fact]
    public void FullyAppliedRecovery_HasNoOverflow()
    {
        var result =
            new ResourceRecoveryResult(
                new ResourceId(1),
                requestedRecovery: 100d,
                actualRecovery: 100d);

        Assert.Equal(
            0d,
            result.Overflow);
    }

    [Fact]
    public void CompletelyUnavailableRecovery_BecomesOverflow()
    {
        var result =
            new ResourceRecoveryResult(
                new ResourceId(1),
                requestedRecovery: 100d,
                actualRecovery: 0d);

        Assert.Equal(
            100d,
            result.Overflow);
    }

    [Fact]
    public void ZeroRecovery_IsValid()
    {
        var result =
            new ResourceRecoveryResult(
                new ResourceId(1),
                requestedRecovery: 0d,
                actualRecovery: 0d);

        Assert.Equal(0d, result.RequestedRecovery);
        Assert.Equal(0d, result.ActualRecovery);
        Assert.Equal(0d, result.Overflow);
    }

    [Fact]
    public void ActualRecoveryAboveRequestedRecovery_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceRecoveryResult(
                    new ResourceId(1),
                    requestedRecovery: 100d,
                    actualRecovery: 101d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1d)]
    public void InvalidRequestedRecovery_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceRecoveryResult(
                    new ResourceId(1),
                    requestedRecovery: value,
                    actualRecovery: 0d));
    }

    [Fact]
    public void InvalidResourceId_Throws()
    {
        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceRecoveryResult(
                    id,
                    requestedRecovery: 0d,
                    actualRecovery: 0d));
    }
}