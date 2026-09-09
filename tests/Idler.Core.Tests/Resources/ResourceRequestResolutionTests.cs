using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceRequestResolutionTests
{
    [Fact]
    public void LossRequest_ForDifferentResource_IsRejected()
    {
        var life =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                new ResourceId(2),
                amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Preview(
                    life,
                    request));

        Assert.Equal(
            100d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);
    }

    [Fact]
    public void RecoveryRequest_ForDifferentResource_IsRejected()
    {
        var life =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                new ResourceId(2),
                amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Preview(
                    life,
                    request));

        Assert.Equal(
            50d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);
    }

    [Fact]
    public void LossPreview_PreservesOriginalRequest()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 25d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        Assert.Equal(
            request,
            preview.Request);

        Assert.Equal(
            request.ResourceId,
            preview.Result.ResourceId);

        Assert.Equal(
            request.Amount,
            preview.Result.RequestedLoss);
    }

    [Fact]
    public void RecoveryPreview_PreservesOriginalRequest()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 25d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            request,
            preview.Request);

        Assert.Equal(
            request.ResourceId,
            preview.Result.ResourceId);

        Assert.Equal(
            request.Amount,
            preview.Result.RequestedRecovery);
    }

    [Fact]
    public void LossCommit_ReturnsExactlyPreviewedResult()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 40d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request,
                preventedLoss: 20d);

        var entry =
            ResourceLossOperations.Commit(
                state,
                preview);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            40d,
            entry.Result.ActualLoss);

        Assert.Equal(
            40d,
            entry.Result.Shortfall);

        Assert.Equal(
            0d,
            state.Current);
    }

    [Fact]
    public void RecoveryCommit_ReturnsExactlyPreviewedResult()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 80d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                state,
                preview);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            20d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            30d,
            entry.Result.Overflow);

        Assert.Equal(
            100d,
            state.Current);
    }
}