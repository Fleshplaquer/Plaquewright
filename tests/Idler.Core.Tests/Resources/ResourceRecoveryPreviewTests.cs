using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceRecoveryPreviewTests
{
    [Fact]
    public void PreviewRecovery_DoesNotMutateState()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 50d);

        Assert.Equal(
            25d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);

        Assert.Equal(
            25d,
            preview.CurrentBefore);

        Assert.Equal(
            75d,
            preview.CurrentAfter);

        Assert.Equal(
            50d,
            preview.Result.ActualRecovery);
    }

    [Fact]
    public void PreviewRecovery_AccountsForOverflow()
    {
        var state =
            CreateState(
                current: 80d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 50d);

        Assert.Equal(
            20d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            30d,
            preview.Result.Overflow);

        Assert.Equal(
            100d,
            preview.CurrentAfter);
    }

    [Fact]
    public void CommitRecovery_AppliesPreviewedState()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 50d);

        ResourceStateOperations.Commit(
            state,
            preview);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
    }

    [Fact]
    public void CommitRecovery_RejectsStalePreview()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var stalePreview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 50d);

        var otherPreview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 10d);

        ResourceStateOperations.Commit(
            state,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceStateOperations.Commit(
                    state,
                    stalePreview));

        Assert.Equal(
            35d,
            state.Current);
    }

    [Fact]
    public void CommitRecovery_RejectsDifferentStateWithSameResourceId()
    {
        var first =
            CreateState(
                current: 25d,
                maximum: 100d);

        var second =
            CreateState(
                current: 25d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewRecovery(
                first,
                requestedRecovery: 50d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceStateOperations.Commit(
                    second,
                    preview));

        Assert.Equal(
            25d,
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