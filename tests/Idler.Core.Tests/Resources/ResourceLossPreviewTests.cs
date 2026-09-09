using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceLossPreviewTests
{
    [Fact]
    public void PreviewLoss_DoesNotMutateState()
    {
        var state =
            CreateState(
                current: 75d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 50d);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);

        Assert.Equal(
            75d,
            preview.CurrentBefore);

        Assert.Equal(
            25d,
            preview.CurrentAfter);

        Assert.Equal(
            50d,
            preview.Result.ActualLoss);
    }

    [Fact]
    public void PreviewLoss_AccountsForPrevention()
    {
        var state =
            CreateState(
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 100d,
                preventedLoss: 30d);

        Assert.Equal(
            70d,
            preview.Result.ActualLoss);

        Assert.Equal(
            30d,
            preview.Result.PreventedLoss);

        Assert.Equal(
            0d,
            preview.Result.Shortfall);

        Assert.Equal(
            30d,
            preview.CurrentAfter);
    }

    [Fact]
    public void PreviewLoss_AccountsForInsufficientCurrentResource()
    {
        var state =
            CreateState(
                current: 40d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 100d,
                preventedLoss: 20d);

        Assert.Equal(
            40d,
            preview.Result.ActualLoss);

        Assert.Equal(
            40d,
            preview.Result.Shortfall);

        Assert.Equal(
            0d,
            preview.CurrentAfter);
    }

    [Fact]
    public void CommitLoss_AppliesPreviewedState()
    {
        var state =
            CreateState(
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 25d);

        ResourceStateOperations.Commit(
            state,
            preview);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            100d,
            state.Maximum);

        Assert.Equal(
            1UL,
            state.Revision);
    }

    [Fact]
    public void CommitLoss_RejectsStalePreview()
    {
        var state =
            CreateState(
                current: 100d,
                maximum: 100d);

        var stalePreview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 25d);

        var otherPreview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 10d);

        ResourceStateOperations.Commit(
            state,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceStateOperations.Commit(
                    state,
                    stalePreview));

        Assert.Equal(
            90d,
            state.Current);
    }

    [Fact]
    public void CommitLoss_RejectsDifferentStateWithSameResourceId()
    {
        var first =
            CreateState(
                current: 100d,
                maximum: 100d);

        var second =
            CreateState(
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                first,
                requestedLoss: 25d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceStateOperations.Commit(
                    second,
                    preview));

        Assert.Equal(
            100d,
            second.Current);
    }

    private static ResourceState CreateState(
        double current,
        double maximum)
    {
        return new ResourceState(
            new ResourceId(1),
            current,
            maximum);
    }
}