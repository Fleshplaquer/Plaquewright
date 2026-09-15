using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceRecoveryRequestTests
{
    [Fact]
    public void Constructor_PreservesValues()
    {
        var id =
            new ResourceId(1);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.Leech);

        var request =
            new ResourceRecoveryRequest(
                id,
                40d,
                provenance);

        Assert.Equal(
            id,
            request.ResourceId);

        Assert.Equal(
            40d,
            request.Amount);

        Assert.Equal(
            provenance,
            request.Provenance);
    }

    [Fact]
    public void ZeroAmount_IsValid()
    {
        var request =
            new ResourceRecoveryRequest(
                new ResourceId(1),
                0d,
                Recovery());

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
                new ResourceRecoveryRequest(
                    id,
                    10d,
                    Recovery()));
    }

    [Fact]
    public void DefaultProvenance_Throws()
    {
        ResourceOperationProvenance provenance =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceRecoveryRequest(
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
                new ResourceRecoveryRequest(
                    new ResourceId(1),
                    amount,
                    Recovery()));
    }

    private static ResourceOperationProvenance Recovery()
    {
        return new ResourceOperationProvenance(
            ResourceOperationCause.Recovery);
    }
}