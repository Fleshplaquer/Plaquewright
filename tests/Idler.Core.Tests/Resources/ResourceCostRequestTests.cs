using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceCostRequestTests
{
    [Fact]
    public void Constructor_PreservesValues()
    {
        var id =
            new ResourceId(1);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.SkillCost);

        var request =
            new ResourceCostRequest(
                id,
                25d,
                provenance);

        Assert.Equal(
            id,
            request.ResourceId);

        Assert.Equal(
            25d,
            request.Amount);

        Assert.Equal(
            provenance,
            request.Provenance);
    }

    [Fact]
    public void ZeroAmount_IsValid()
    {
        var request =
            new ResourceCostRequest(
                new ResourceId(1),
                0d,
                SkillCost());

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
                new ResourceCostRequest(
                    id,
                    10d,
                    SkillCost()));
    }

    [Fact]
    public void DefaultProvenance_Throws()
    {
        ResourceOperationProvenance provenance =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceCostRequest(
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
                new ResourceCostRequest(
                    new ResourceId(1),
                    amount,
                    SkillCost()));
    }

    private static ResourceOperationProvenance SkillCost()
    {
        return new ResourceOperationProvenance(
            ResourceOperationCause.SkillCost);
    }
}