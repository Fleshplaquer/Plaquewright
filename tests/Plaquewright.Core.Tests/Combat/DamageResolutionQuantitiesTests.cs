using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResolutionQuantitiesTests
{
    [Fact]
    public void Create_PreservesEveryDamageStage()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var quantities =
            DamageResolutionTestFactory.Create(
                incoming,
                postMitigationAmount: 80d,
                postTakenScalingAmount: 90d,
                damageTakenAmount: 70d);

        Assert.Equal(
            100d,
            quantities.Incoming.Amount);

        Assert.Equal(
            80d,
            quantities.PostMitigation.Amount);

        Assert.Equal(
            90d,
            quantities.PostTakenScaling.Amount);

        Assert.Equal(
            70d,
            quantities.Taken.Amount);
    }

    [Fact]
    public void Create_WithProtectionRouting_UsesFinalPrimaryPathDamageAsDamageTaken()
    {
        var incoming =
            new IncomingDamage(
                100d);

        var protectionResolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 80d,
                [
                    new ProtectionAssignmentRequest(
                    requestedFraction: 0.5d)
                ]);

        var protectionRouting =
            ProtectionAssignmentRoutingStarter.Start(
                protectionResolution);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    protectionRouting.Lanes[0].ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack)
            .Resolve(
                actualResourceUnitsSpent: 30d);

        protectionRouting =
            protectionRouting.Apply(
                protectionResolution.Assignments[0],
                ProtectionShortfallRouter.Route(
                    financing));

        Assert.True(
            protectionRouting.IsComplete);

        Assert.Equal(
            50d,
            protectionRouting.PrimaryPathDamage);

        var quantities =
            DamageResolutionQuantities.Create(
                incoming,
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                protectionRouting);

        Assert.Equal(
            100d,
            quantities.Incoming.Amount);

        Assert.Equal(
            90d,
            quantities.PostMitigation.Amount);

        Assert.Equal(
            80d,
            quantities.PostTakenScaling.Amount);

        Assert.Equal(
            50d,
            quantities.Taken.Amount);
    }

    [Fact]
    public void Create_DoesNotRequirePreProtectionStagesToBeNumericallyMonotonic()
    {
        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 125d,
                postTakenScalingAmount: 75d,
                damageTakenAmount: 60d);

        Assert.Equal(
            100d,
            quantities.Incoming.Amount);

        Assert.Equal(
            125d,
            quantities.PostMitigation.Amount);

        Assert.Equal(
            75d,
            quantities.PostTakenScaling.Amount);

        Assert.Equal(
            60d,
            quantities.Taken.Amount);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInvalidPostMitigationAmount_Throws(
        double amount)
    {
        var protectionRouting =
    DamageResolutionTestFactory
        .CreateCompletedProtectionRouting(
            postTakenScalingAmount: 80d,
            damageTakenAmount: 70d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: amount,
                    postTakenScalingAmount: 80d,
                    protectionRouting));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithInvalidPostTakenScalingAmount_Throws(
        double amount)
    {
        var protectionRouting =
    DamageResolutionTestFactory
        .CreateCompletedProtectionRouting(
            postTakenScalingAmount: 80d,
            damageTakenAmount: 70d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 90d,
                    postTakenScalingAmount: amount,
                    protectionRouting));
    }

    [Fact]
    public void Create_WithProtectionRoutingForDifferentPostTakenScalingDamage_Throws()
    {
        var protectionRouting =
            DamageResolutionTestFactory
                .CreateCompletedProtectionRouting(
                    postTakenScalingAmount: 80d,
                    damageTakenAmount: 70d);

        Assert.Throws<InvalidOperationException>(
            () =>
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 90d,
                    postTakenScalingAmount: 81d,
                    protectionRouting));
    }

    [Fact]
    public void Snapshot_IsIndependentFromLaterResourceLoss()
    {
        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var resourceLoss =
            quantities.Taken.AdvanceToActualResourceLoss(
                25d);

        Assert.Equal(
            60d,
            quantities.Taken.Amount);

        Assert.Equal(
            25d,
            resourceLoss.Amount);
    }
}