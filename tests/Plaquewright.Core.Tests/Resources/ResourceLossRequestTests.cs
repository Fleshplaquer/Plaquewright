using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceLossRequestTests
{
    [Fact]
    public void Constructor_PreservesValues()
    {
        var id =
            new ResourceId(1);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var request =
            new ResourceLossRequest(
                id,
                50d,
                provenance);

        Assert.Equal(
            id,
            request.ResourceId);

        Assert.Equal(
            50d,
            request.Amount);

        Assert.Equal(
            provenance,
            request.Provenance);
    }

    [Fact]
    public void ZeroAmount_IsValid()
    {
        var request =
            new ResourceLossRequest(
                new ResourceId(1),
                0d,
                Direct());

        Assert.Equal(
            0d,
            request.Amount);
    }

    [Fact]
    public void DefaultResourceId_Throws()
    {
        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceLossRequest(
                    id,
                    10d,
                    Direct()));
    }

    [Fact]
    public void DefaultProvenance_Throws()
    {
        ResourceOperationProvenance provenance =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceLossRequest(
                    new ResourceId(1),
                    10d,
                    provenance));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidAmount_Throws(
        double amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceLossRequest(
                    new ResourceId(1),
                    amount,
                    Direct()));
    }

    private static ResourceOperationProvenance Direct()
    {
        return new ResourceOperationProvenance(
            ResourceOperationCause.Direct);
    }
}