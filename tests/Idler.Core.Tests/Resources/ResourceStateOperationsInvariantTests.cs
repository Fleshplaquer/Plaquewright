using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateOperationsInvariantTests
{
    [Fact]
    public void RepresentativeLossOperations_PreserveAllInvariants()
    {
        var currentValues = new[]
        {
            0d,
            1d,
            25d,
            50d,
            100d
        };

        var requestedValues = new[]
        {
            0d,
            1d,
            10d,
            50d,
            100d,
            200d
        };

        foreach (var current in currentValues)
        {
            foreach (var requested in requestedValues)
            {
                var preventedValues = new[]
                {
                    0d,
                    requested / 2d,
                    requested
                };

                foreach (var prevented in preventedValues)
                {
                    var state =
                        new ResourceState(
                            new ResourceId(1),
                            current,
                            maximum: 100d);

                    var preview =
                        ResourceStateOperations.PreviewLoss(
                            state,
                            requested,
                            prevented);

                    // Preview must not mutate state.
                    Assert.Equal(
                        current,
                        state.Current);

                    Assert.Equal(
                        0UL,
                        state.Revision);

                    // Accounting invariant.
                    Assert.Equal(
                        preview.Result.RequestedLoss,
                        preview.Result.PreventedLoss +
                        preview.Result.ActualLoss +
                        preview.Result.Shortfall);

                    // State-delta invariant.
                    Assert.Equal(
                        preview.CurrentBefore -
                        preview.Result.ActualLoss,
                        preview.CurrentAfter);

                    Assert.True(
                        preview.CurrentAfter >= 0d);

                    Assert.True(
                        preview.CurrentAfter <=
                        preview.Maximum);

                    ResourceStateOperations.Commit(
                        state,
                        preview);

                    Assert.Equal(
                        preview.CurrentAfter,
                        state.Current);

                    Assert.Equal(
                        preview.Maximum,
                        state.Maximum);

                    Assert.Equal(
                        1UL,
                        state.Revision);
                }
            }
        }
    }

    [Fact]
    public void RepresentativeRecoveryOperations_PreserveAllInvariants()
    {
        var currentValues = new[]
        {
            0d,
            1d,
            25d,
            50d,
            100d
        };

        var requestedValues = new[]
        {
            0d,
            1d,
            10d,
            50d,
            100d,
            200d
        };

        foreach (var current in currentValues)
        {
            foreach (var requested in requestedValues)
            {
                var state =
                    new ResourceState(
                        new ResourceId(1),
                        current,
                        maximum: 100d);

                var preview =
                    ResourceStateOperations.PreviewRecovery(
                        state,
                        requested);

                // Preview must not mutate state.
                Assert.Equal(
                    current,
                    state.Current);

                Assert.Equal(
                    0UL,
                    state.Revision);

                // Accounting invariant.
                Assert.Equal(
                    preview.Result.RequestedRecovery,
                    preview.Result.ActualRecovery +
                    preview.Result.Overflow);

                // State-delta invariant.
                Assert.Equal(
                    preview.CurrentBefore +
                    preview.Result.ActualRecovery,
                    preview.CurrentAfter);

                Assert.True(
                    preview.CurrentAfter >= 0d);

                Assert.True(
                    preview.CurrentAfter <=
                    preview.Maximum);

                ResourceStateOperations.Commit(
                    state,
                    preview);

                Assert.Equal(
                    preview.CurrentAfter,
                    state.Current);

                Assert.Equal(
                    preview.Maximum,
                    state.Maximum);

                Assert.Equal(
                    1UL,
                    state.Revision);
            }
        }
    }

    [Fact]
    public void CommitMakesEveryOlderPreviewStaleRegardlessOfOperationType()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var lossPreview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 20d);

        var recoveryPreview =
            ResourceStateOperations.PreviewRecovery(
                state,
                requestedRecovery: 20d);

        ResourceStateOperations.Commit(
            state,
            recoveryPreview);

        Assert.Equal(
            70d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceStateOperations.Commit(
                    state,
                    lossPreview));

        Assert.Equal(
            70d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
    }

    [Fact]
    public void SequentialOperations_RequireFreshPreview()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var first =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 30d);

        ResourceStateOperations.Commit(
            state,
            first);

        var second =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 20d);

        ResourceStateOperations.Commit(
            state,
            second);

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            2UL,
            state.Revision);
    }

    [Fact]
    public void ZeroAmountCommit_IsStillAnExplicitStateCommit()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceStateOperations.PreviewLoss(
                state,
                requestedLoss: 0d);

        ResourceStateOperations.Commit(
            state,
            preview);

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
    }
}