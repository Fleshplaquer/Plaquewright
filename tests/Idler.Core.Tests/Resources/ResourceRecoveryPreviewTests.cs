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

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            25d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);

        Assert.Equal(
            request,
            preview.Request);

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

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            20d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            30d,
            preview.Result.Overflow);

        Assert.Equal(
            100d,
            preview.CurrentAfter);

        Assert.Equal(
            80d,
            state.Current);
    }

    [Fact]
    public void FullyAvailableRecovery_HasNoOverflow()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            50d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            0d,
            preview.Result.Overflow);

        Assert.Equal(
            75d,
            preview.CurrentAfter);
    }

    [Fact]
    public void RecoveryAtMaximum_BecomesCompleteOverflow()
    {
        var state =
            CreateState(
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            0d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            50d,
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

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
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

        Assert.Equal(
            50d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            0d,
            entry.Result.Overflow);
    }

    [Fact]
    public void CommitRecovery_RejectsStalePreview()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var stalePreview =
            ResourceRecoveryOperations.Preview(
                state,
                new ResourceRecoveryRequest(
                    state.Id,
                    amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery)));

        var otherPreview =
            ResourceRecoveryOperations.Preview(
                state,
                new ResourceRecoveryRequest(
                    state.Id,
                    amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery)));

        ResourceRecoveryOperations.Commit(
            state,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    state,
                    stalePreview));

        Assert.Equal(
            35d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
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
            ResourceRecoveryOperations.Preview(
                first,
                new ResourceRecoveryRequest(
                    first.Id,
                    amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    second,
                    preview));

        Assert.Equal(
            25d,
            second.Current);

        Assert.Equal(
            0UL,
            second.Revision);
    }

    [Fact]
    public void ZeroRecoveryCommit_IsStillExplicitCommit()
    {
        var state =
            CreateState(
                current: 25d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 0d, new ResourceOperationProvenance(
        ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                state,
                preview);

        Assert.Equal(
            25d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            0d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            0d,
            entry.Result.Overflow);
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