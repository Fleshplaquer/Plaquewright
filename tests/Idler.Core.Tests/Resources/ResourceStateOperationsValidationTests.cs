using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateOperationsValidationTests
{
    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void LossRequest_WithInvalidAmount_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceLossRequest(
                    new ResourceId(1),
                    value, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PreviewLoss_WithInvalidPreventedLoss_Throws(
        double value)
    {
        var state =
            CreateState();

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceLossOperations.Preview(
                    state,
                    request,
                    preventedLoss: value));
    }

    [Fact]
    public void PreviewLoss_WithPreventionAboveRequestedLoss_Throws()
    {
        var state =
            CreateState();

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ResourceLossOperations.Preview(
                    state,
                    request,
                    preventedLoss: 101d));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RecoveryRequest_WithInvalidAmount_Throws(
        double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceRecoveryRequest(
                    new ResourceId(1),
                    value, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));
    }

    [Fact]
    public void PreviewLoss_WithNullState_Throws()
    {
        var request =
            new ResourceLossRequest(
                new ResourceId(1),
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<ArgumentNullException>(
            () =>
                ResourceLossOperations.Preview(
                    null!,
                    request));
    }

    [Fact]
    public void PreviewRecovery_WithNullState_Throws()
    {
        var request =
            new ResourceRecoveryRequest(
                new ResourceId(1),
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<ArgumentNullException>(
            () =>
                ResourceRecoveryOperations.Preview(
                    null!,
                    request));
    }

    [Fact]
    public void PreviewLoss_WithDifferentResourceId_Throws()
    {
        var state =
            CreateState();

        var request =
            new ResourceLossRequest(
                new ResourceId(2),
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Preview(
                    state,
                    request));
    }

    [Fact]
    public void PreviewRecovery_WithDifferentResourceId_Throws()
    {
        var state =
            CreateState();

        var request =
            new ResourceRecoveryRequest(
                new ResourceId(2),
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Preview(
                    state,
                    request));
    }

    private static ResourceState CreateState()
    {
        return new ResourceState(
            new ResourceId(1),
            current: 50d,
            maximum: 100d);
    }
}