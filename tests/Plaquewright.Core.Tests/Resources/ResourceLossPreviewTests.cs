using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceLossPreviewTests
{
    [Fact]
    public void PreviewLoss_DoesNotMutateState()
    {
        var state =
            CreateState(
                current: 75d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);

        Assert.Equal(
            request,
            preview.Request);

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

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request,
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

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request,
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

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 25d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        var entry =
            ResourceLossOperations.Commit(
                new EntityId(1UL),
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
            25d,
            entry.Result.ActualLoss);
    }

    [Fact]
    public void CommitLoss_RejectsStalePreview()
    {
        var state =
            CreateState(
                current: 100d,
                maximum: 100d);

        var stalePreview =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 25d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct)));

        var otherPreview =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct)));

        ResourceLossOperations.Commit(
            new EntityId(1UL),
            state,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    new EntityId(1UL),
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
            ResourceLossOperations.Preview(
                first,
                new ResourceLossRequest(
                    first.Id,
                    amount: 25d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    new EntityId(1UL),
                    second,
                    preview));

        Assert.Equal(
            100d,
            second.Current);

        Assert.Equal(
            0UL,
            second.Revision);
    }

    [Fact]
    public void ZeroLossCommit_IsStillExplicitCommit()
    {
        var state =
            CreateState(
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 0d, new ResourceOperationProvenance(
        ResourceOperationCause.Direct)));

        var entry =
            ResourceLossOperations.Commit(
                new EntityId(1UL),
                state,
                preview);

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            0d,
            entry.Result.ActualLoss);
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