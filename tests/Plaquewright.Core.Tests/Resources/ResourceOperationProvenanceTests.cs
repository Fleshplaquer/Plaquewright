using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Resources;

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

        Assert.False(
            provenance.HasGameplayExecution);

        Assert.Null(
            provenance.GameplayExecutionId);
    }

    [Fact]
    public void Constructor_WithGameplayExecutionId_PreservesCausalExecution()
    {
        var gameplayExecutionId =
            new ExecutionId(42UL);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived,
                gameplayExecutionId);

        Assert.True(
            provenance.IsValid);

        Assert.True(
            provenance.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            provenance.GameplayExecutionId.GetValueOrDefault());
    }

    [Fact]
    public void Constructor_WithInvalidGameplayExecutionId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived,
                    default));
    }

    [Fact]
    public void SameCause_WithDifferentGameplayExecutions_ProducesDifferentProvenance()
    {
        var first =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived,
                new ExecutionId(1UL));

        var second =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived,
                new ExecutionId(2UL));

        Assert.NotEqual(
            first,
            second);
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