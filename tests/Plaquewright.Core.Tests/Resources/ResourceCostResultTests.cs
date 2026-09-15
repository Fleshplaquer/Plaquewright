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
                requestedCost: 50d,
                 actualCost: 50d);

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
    public void Constructor_PreservesActualCostSeparatelyFromRequestedCost()
    {
        var result =
            new ResourceCostResult(
                new ResourceId(1),
                requestedCost: 0.1d,
                actualCost: 0.09999999999999d);

        Assert.Equal(
            0.1d,
            result.RequestedCost);

        Assert.Equal(
            0.09999999999999d,
            result.ActualCost);
    }

    [Fact]
    public void ZeroCost_IsValid()
    {
        var result =
            new ResourceCostResult(
                new ResourceId(1),
                requestedCost: 0d,
                 actualCost: 0d);

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
                    requestedCost: 10d,
                     actualCost: 0d));
    }
    [Fact]
    public void ActualCostGreaterThanRequestedCost_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceCostResult(
                    new ResourceId(1),
                    requestedCost: 10d,
                    actualCost: 11d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidActualCost_Throws(
        double actualCost)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceCostResult(
                    new ResourceId(1),
                    requestedCost: 10d,
                    actualCost: actualCost));
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
                    requestedCost: value,
                     actualCost: 0d));
    }
}