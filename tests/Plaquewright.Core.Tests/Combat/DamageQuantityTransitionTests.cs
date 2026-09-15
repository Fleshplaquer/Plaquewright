using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageQuantityTransitionTests
{
    [Fact]
    public void Pipeline_CanAdvanceThroughEverySemanticStage()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                80d);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                90d);

        var damageTaken =
            postTakenScaling.AdvanceToDamageTaken(
                70d);

        var actualResourceLoss =
            damageTaken.AdvanceToActualResourceLoss(
                60d);

        Assert.Equal(
            100d,
            incoming.Amount);

        Assert.Equal(
            80d,
            postMitigation.Amount);

        Assert.Equal(
            90d,
            postTakenScaling.Amount);

        Assert.Equal(
            70d,
            damageTaken.Amount);

        Assert.Equal(
            60d,
            actualResourceLoss.Amount);
    }

    [Fact]
    public void PostMitigationDamage_IsNotRequiredToBeLessThanIncomingDamage()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                125d);

        Assert.Equal(
            125d,
            postMitigation.Amount);
    }

    [Fact]
    public void PostTakenScalingDamage_IsNotRequiredToBeMonotonic()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                125d);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                75d);

        Assert.Equal(
            75d,
            postTakenScaling.Amount);
    }

    [Fact]
    public void DamageTaken_CanBeLowerThanPostTakenScalingDamage()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                100d);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                100d);

        var damageTaken =
            postTakenScaling.AdvanceToDamageTaken(
                40d);

        Assert.Equal(
            40d,
            damageTaken.Amount);
    }

    [Fact]
    public void ActualResourceLoss_HasNoNumericOrderingContractWithDamageTaken()
    {
        var incoming =
            new IncomingDamage(
                10d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                10d);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                10d);

        var damageTaken =
            postTakenScaling.AdvanceToDamageTaken(
                10d);

        var actualResourceLoss =
            damageTaken.AdvanceToActualResourceLoss(
                25d);

        Assert.Equal(
            10d,
            damageTaken.Amount);

        Assert.Equal(
            25d,
            actualResourceLoss.Amount);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceToPostMitigation_RejectsInvalidResolvedAmount(
        double amount)
    {
        var incoming =
            new IncomingDamage(
                100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                incoming.AdvanceToPostMitigation(
                    amount));
    }

    [Fact]
    public void AdvancingPipeline_DoesNotChangePreviousQuantities()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var postMitigation =
            incoming.AdvanceToPostMitigation(
                80d);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                60d);

        Assert.Equal(
            100d,
            incoming.Amount);

        Assert.Equal(
            80d,
            postMitigation.Amount);

        Assert.Equal(
            60d,
            postTakenScaling.Amount);
    }
}