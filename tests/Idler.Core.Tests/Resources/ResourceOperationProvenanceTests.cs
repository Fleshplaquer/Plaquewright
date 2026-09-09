using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceOperationProvenanceTests
{
    [Theory]
    [InlineData(ResourceOperationCause.Direct)]
    [InlineData(ResourceOperationCause.DamageDerived)]
    [InlineData(ResourceOperationCause.SkillCost)]
    [InlineData(ResourceOperationCause.Sacrifice)]
    [InlineData(ResourceOperationCause.Recovery)]
    [InlineData(ResourceOperationCause.Regeneration)]
    [InlineData(ResourceOperationCause.Leech)]
    [InlineData(ResourceOperationCause.Transfer)]
    [InlineData(ResourceOperationCause.ProtectionFinancing)]
    public void Constructor_WithKnownCause_IsValid(
        ResourceOperationCause cause)
    {
        var provenance =
            new ResourceOperationProvenance(
                cause);

        Assert.True(
            provenance.IsValid);

        Assert.Equal(
            cause,
            provenance.Cause);
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        ResourceOperationProvenance provenance =
            default;

        Assert.False(
            provenance.IsValid);
    }

    [Fact]
    public void ZeroCause_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceOperationProvenance(
                    (ResourceOperationCause)0));
    }

    [Fact]
    public void UnknownCause_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceOperationProvenance(
                    (ResourceOperationCause)999));
    }

    [Fact]
    public void SameCause_ProducesEqualProvenance()
    {
        var first =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var second =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        Assert.Equal(
            first,
            second);
    }

    [Fact]
    public void DifferentCauses_ProduceDifferentProvenance()
    {
        var damage =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var sacrifice =
            new ResourceOperationProvenance(
                ResourceOperationCause.Sacrifice);

        Assert.NotEqual(
            damage,
            sacrifice);
    }
}