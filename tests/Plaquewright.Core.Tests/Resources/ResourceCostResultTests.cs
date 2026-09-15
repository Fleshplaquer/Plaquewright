using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostResultTests
{
    [Fact]
    public void Constructor_PreservesResourceAndCost()
    {
        var id =
            new ResourceId(1);

        var result =
            new ResourceCostResult(
                id,
                requestedCost: 50d);

        Assert.Equal(
            id,
            result.ResourceId);

        Assert.Equal(
            50d,
            result.RequestedCost);

        Assert.Equal(
            50d,
            result.ActualCost);
    }

    [Fact]
    public void ActualCost_AlwaysEqualsRequestedCost()
    {
        var result =
            new ResourceCostResult(
                new ResourceId(1),
                requestedCost: 75d);

        Assert.Equal(
            result.RequestedCost,
            result.ActualCost);
    }

    [Fact]
    public void ZeroCost_IsValid()
    {
        var result =
            new ResourceCostResult(
                new ResourceId(1),
                requestedCost: 0d);

        Assert.Equal(
            0d,
            result.RequestedCost);

        Assert.Equal(
            0d,
            result.ActualCost);
    }

    [Fact]
    public void InvalidResourceId_Throws()
    {
        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceCostResult(
                    id,
                    requestedCost: 10d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidRequestedCost_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceCostResult(
                    new ResourceId(1),
                    requestedCost: value));
    }
}