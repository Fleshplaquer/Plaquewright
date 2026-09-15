using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceQuantityMathTests
{
    [Fact]
    public void LossSmallerThanRepresentableStep_BecomesZeroActualLoss()
    {
        var current =
            1e16d;

        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterLoss(
                current,
                requestedLoss: 1d,
                out var actualLoss);

        Assert.Equal(
            current,
            currentAfter);

        Assert.Equal(
            0d,
            actualLoss);
    }

    [Fact]
    public void LossWhoseRoundedSubtractionWouldOvershoot_IsConservative()
    {
        var current =
            1e16d;

        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterLoss(
                current,
                requestedLoss: 3d,
                out var actualLoss);

        Assert.Equal(
            2d,
            actualLoss);

        Assert.Equal(
            current - 2d,
            currentAfter);

        Assert.True(
            actualLoss <=
            3d);
    }

    [Fact]
    public void ExactlyRepresentableLoss_IsPreserved()
    {
        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterLoss(
                current: 100d,
                requestedLoss: 25d,
                out var actualLoss);

        Assert.Equal(
            75d,
            currentAfter);

        Assert.Equal(
            25d,
            actualLoss);
    }
    [Fact]
    public void RecoverySmallerThanRepresentableStep_BecomesZeroActualRecovery()
    {
        var current =
            1e16d;

        var maximum =
            current + 10d;

        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterRecovery(
                current,
                maximum,
                requestedRecovery: 1d,
                out var actualRecovery);

        Assert.Equal(
            current,
            currentAfter);

        Assert.Equal(
            0d,
            actualRecovery);
    }

    [Fact]
    public void RecoveryWhoseRoundedAdditionWouldOvershoot_IsConservative()
    {
        var current =
            1e16d;

        var maximum =
            current + 10d;

        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterRecovery(
                current,
                maximum,
                requestedRecovery: 3d,
                out var actualRecovery);

        Assert.Equal(
            2d,
            actualRecovery);

        Assert.Equal(
            current + 2d,
            currentAfter);

        Assert.True(
            actualRecovery <=
            3d);
    }

    [Fact]
    public void ExactlyRepresentableRecovery_IsPreserved()
    {
        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterRecovery(
                current: 25d,
                maximum: 100d,
                requestedRecovery: 50d,
                out var actualRecovery);

        Assert.Equal(
            75d,
            currentAfter);

        Assert.Equal(
            50d,
            actualRecovery);
    }

    [Fact]
    public void RecoveryLargerThanAvailableCapacity_IsBoundedByMaximum()
    {
        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterRecovery(
                current: 80d,
                maximum: 100d,
                requestedRecovery: 50d,
                out var actualRecovery);

        Assert.Equal(
            100d,
            currentAfter);

        Assert.Equal(
            20d,
            actualRecovery);
    }

    [Fact]
    public void Cost_WithOrdinaryFloatingPointDifference_IsRepresentable()
    {
        var representable =
            ResourceQuantityMath.TryCalculateCurrentAfterCost(
                current: 100d,
                requestedCost: 0.1d,
                out var currentAfter,
                out var actualCost);

        Assert.True(
            representable);

        Assert.True(
            currentAfter <
            100d);

        Assert.True(
            actualCost >
            0d);

        var relativeError =
            Math.Abs(
                actualCost -
                0.1d) /
            0.1d;

        Assert.True(
            relativeError <=
            1e-9d);
    }

    [Fact]
    public void Cost_SmallerThanRepresentableStateChange_IsNotRepresentable()
    {
        var current =
            1e16d;

        var representable =
            ResourceQuantityMath.TryCalculateCurrentAfterCost(
                current,
                requestedCost: 1d,
                out var currentAfter,
                out var actualCost);

        Assert.False(
            representable);

        Assert.Equal(
            current,
            currentAfter);

        Assert.Equal(
            0d,
            actualCost);
    }

    [Fact]
    public void Cost_WithMeaningfulRepresentationError_IsNotRepresentable()
    {
        var current =
            1e16d;

        var representable =
            ResourceQuantityMath.TryCalculateCurrentAfterCost(
                current,
                requestedCost: 3d,
                out var currentAfter,
                out var actualCost);

        Assert.False(
            representable);

        Assert.Equal(
            current,
            currentAfter);

        Assert.Equal(
            0d,
            actualCost);
    }

    [Fact]
    public void LossLargerThanCurrent_IsBoundedByCurrent()
    {
        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterLoss(
                current: 40d,
                requestedLoss: 100d,
                out var actualLoss);

        Assert.Equal(
            0d,
            currentAfter);

        Assert.Equal(
            40d,
            actualLoss);
    }
}