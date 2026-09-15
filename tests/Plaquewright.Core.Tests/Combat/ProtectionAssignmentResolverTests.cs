using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionAssignmentResolverTests
{
    [Fact]
    public void FiftyPercentAssignment_SplitsDamageBetweenPrimaryAndProtection()
    {
        var request =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.5d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 120d,
                [request]);

        Assert.Equal(
            120d,
            result.InitialDamage);

        Assert.Equal(
            0.5d,
            result.RequestedFractionTotal);

        Assert.Equal(
            0.5d,
            result.AppliedFractionTotal);

        Assert.Equal(
            1d,
            result.AssignmentScale);

        Assert.False(
            result.WasScaled);

        Assert.Equal(
            60d,
            result.PrimaryPathDamage);

        Assert.Equal(
            60d,
            result.TotalAssignedDamage);

        var assignment =
            Assert.Single(
                result.Assignments);

        Assert.Same(
            request,
            assignment.Request);

        Assert.Equal(
            0.5d,
            assignment.RequestedFraction);

        Assert.Equal(
            0.5d,
            assignment.AppliedFraction);

        Assert.Equal(
            60d,
            assignment.AssignedDamage);

        Assert.Equal(
            120d,
            result.AccountedDamage);
    }
    [Fact]
    public void TinyLastAssignment_DoesNotBecomeNegativeFromFloatingPointRounding()
    {
        var first =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.1d);

        var second =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.7d);

        var tiny =
            new ProtectionAssignmentRequest(
                requestedFraction: 1e-20d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 10d,
                [
                    first,
                second,
                tiny
                ]);

        Assert.Equal(
            3,
            result.Assignments.Count);

        Assert.True(
            result.Assignments[0].AssignedDamage >=
            0d);

        Assert.True(
            result.Assignments[1].AssignedDamage >=
            0d);

        Assert.Equal(
            0d,
            result.Assignments[2].AssignedDamage);

        var summedAssignments =
            result.Assignments.Sum(
                assignment =>
                    assignment.AssignedDamage);

        Assert.Equal(
            result.TotalAssignedDamage,
            summedAssignments);

        Assert.Equal(
            result.InitialDamage,
            result.PrimaryPathDamage +
            summedAssignments);
    }

    [Fact]
    public void MultipleAssignments_BelowOneHundredPercent_LeavePrimaryDamage()
    {
        var mana =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.3d);

        var other =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.2d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 120d,
                [mana, other]);

        Assert.Equal(
            0.5d,
            result.RequestedFractionTotal);

        Assert.Equal(
            1d,
            result.AssignmentScale);

        Assert.False(
            result.WasScaled);

        Assert.Equal(
            60d,
            result.PrimaryPathDamage);

        Assert.Equal(
            60d,
            result.TotalAssignedDamage);

        Assert.Equal(
            36d,
            result.Assignments[0].AssignedDamage);

        Assert.Equal(
            24d,
            result.Assignments[1].AssignedDamage);

        Assert.Equal(
            120d,
            result.AccountedDamage);
    }

    [Fact]
    public void Assignments_ExactlyOneHundredPercent_LeaveNoPrimaryDamage()
    {
        var first =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.75d);

        var second =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.25d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [first, second]);

        Assert.False(
            result.WasScaled);

        Assert.Equal(
            0d,
            result.PrimaryPathDamage);

        Assert.Equal(
            75d,
            result.Assignments[0].AssignedDamage);

        Assert.Equal(
            25d,
            result.Assignments[1].AssignedDamage);

        Assert.Equal(
            100d,
            result.TotalAssignedDamage);

        Assert.Equal(
            100d,
            result.AccountedDamage);
    }

    [Fact]
    public void Assignments_AboveOneHundredPercent_AreScaledProportionally()
    {
        var first =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.8d);

        var second =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.8d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [first, second]);

        Assert.Equal(
            1.6d,
            result.RequestedFractionTotal);

        Assert.Equal(
            1d,
            result.AppliedFractionTotal);

        Assert.Equal(
            0.625d,
            result.AssignmentScale);

        Assert.True(
            result.WasScaled);

        Assert.Equal(
            0d,
            result.PrimaryPathDamage);

        Assert.Equal(
            0.5d,
            result.Assignments[0].AppliedFraction);

        Assert.Equal(
            0.5d,
            result.Assignments[1].AppliedFraction);

        Assert.Equal(
            50d,
            result.Assignments[0].AssignedDamage);

        Assert.Equal(
            50d,
            result.Assignments[1].AssignedDamage);

        Assert.Equal(
            100d,
            result.AccountedDamage);
    }

    [Fact]
    public void UnequalOversubscription_PreservesRelativeContributionWeights()
    {
        var first =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.75d);

        var second =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.25d);

        var third =
            new ProtectionAssignmentRequest(
                requestedFraction: 0.5d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 150d,
                [first, second, third]);

        Assert.Equal(
            1.5d,
            result.RequestedFractionTotal);

        Assert.True(
            result.WasScaled);

        Assert.Equal(
            0d,
            result.PrimaryPathDamage);

        // Requested weights 0.75 : 0.25 : 0.50
        // normalize to 0.50 : 1/6 : 1/3.

        Assert.Equal(
            75d,
            result.Assignments[0].AssignedDamage);

        Assert.Equal(
            25d,
            result.Assignments[1].AssignedDamage);

        Assert.Equal(
            50d,
            result.Assignments[2].AssignedDamage);

        Assert.Equal(
            150d,
            result.TotalAssignedDamage);

        Assert.Equal(
            150d,
            result.AccountedDamage);
    }

    [Fact]
    public void SingleAssignment_AboveOneHundredPercent_IsCappedByAvailableDamage()
    {
        var request =
            new ProtectionAssignmentRequest(
                requestedFraction: 1.5d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [request]);

        Assert.True(
            result.WasScaled);

        Assert.Equal(
            1.5d,
            result.RequestedFractionTotal);

        Assert.Equal(
            1d,
            result.AppliedFractionTotal);

        Assert.Equal(
            0d,
            result.PrimaryPathDamage);

        Assert.Equal(
            1d,
            result.Assignments[0].AppliedFraction);

        Assert.Equal(
            100d,
            result.Assignments[0].AssignedDamage);
    }

    [Fact]
    public void NoAssignments_LeavesAllDamageOnPrimaryPath()
    {
        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 120d,
                Array.Empty<ProtectionAssignmentRequest>());

        Assert.Equal(
            120d,
            result.PrimaryPathDamage);

        Assert.Equal(
            0d,
            result.TotalAssignedDamage);

        Assert.Equal(
            0d,
            result.RequestedFractionTotal);

        Assert.Equal(
            0d,
            result.AppliedFractionTotal);

        Assert.Equal(
            1d,
            result.AssignmentScale);

        Assert.False(
            result.WasScaled);

        Assert.Empty(
            result.Assignments);

        Assert.Equal(
            120d,
            result.AccountedDamage);
    }

    [Fact]
    public void ZeroFractionAssignment_AssignsNoDamage()
    {
        var request =
            new ProtectionAssignmentRequest(
                requestedFraction: 0d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [request]);

        Assert.Equal(
            100d,
            result.PrimaryPathDamage);

        Assert.Equal(
            0d,
            result.Assignments[0].AssignedDamage);

        Assert.Equal(
            0d,
            result.Assignments[0].AppliedFraction);
    }

    [Fact]
    public void AssignmentResults_PreserveInputOrder()
    {
        var first =
            new ProtectionAssignmentRequest(
                0.1d);

        var second =
            new ProtectionAssignmentRequest(
                0.2d);

        var third =
            new ProtectionAssignmentRequest(
                0.3d);

        var result =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [first, second, third]);

        Assert.Same(
            first,
            result.Assignments[0].Request);

        Assert.Same(
            second,
            result.Assignments[1].Request);

        Assert.Same(
            third,
            result.Assignments[2].Request);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidRequestedFraction_Throws(
        double fraction)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionAssignmentRequest(
                    fraction));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidDamage_Throws(
        double damage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ProtectionAssignmentResolver.Resolve(
                    damage,
                    Array.Empty<ProtectionAssignmentRequest>()));
    }

    [Fact]
    public void NullAssignment_IsRejected()
    {
        ProtectionAssignmentRequest? request =
            null;

        Assert.Throws<ArgumentNullException>(
            () =>
                ProtectionAssignmentResolver.Resolve(
                    damage: 100d,
                    [request!]));
    }
}