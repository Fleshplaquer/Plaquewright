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

                    var request =
                        new ResourceLossRequest(
                            state.Id,
                            requested, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

                    var preview =
                        ResourceLossOperations.Preview(
                            state,
                            request,
                            prevented);

                    // Preview must not mutate state.
                    Assert.Equal(
                        current,
                        state.Current);

                    Assert.Equal(
                        0UL,
                        state.Revision);

                    // Original request is preserved.
                    Assert.Equal(
                        request,
                        preview.Request);

                    // Accounting invariant:
                    //
                    // RequestedLoss
                    // =
                    // PreventedLoss
                    // + ActualLoss
                    // + Shortfall
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
                        double.IsFinite(
                            preview.CurrentAfter));

                    Assert.True(
                        preview.CurrentAfter >= 0d);

                    Assert.True(
                        preview.CurrentAfter <=
                        preview.Maximum);

                    var entry =
                        ResourceLossOperations.Commit(
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

                    Assert.Equal(
                        preview.Result,
                        entry.Result);
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

                var request =
                    new ResourceRecoveryRequest(
                        state.Id,
                        requested, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived));

                var preview =
                    ResourceRecoveryOperations.Preview(
                        state,
                        request);

                // Preview must not mutate state.
                Assert.Equal(
                    current,
                    state.Current);

                Assert.Equal(
                    0UL,
                    state.Revision);

                // Original request is preserved.
                Assert.Equal(
                    request,
                    preview.Request);

                // Accounting invariant:
                //
                // RequestedRecovery
                // =
                // ActualRecovery
                // + Overflow
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
                    double.IsFinite(
                        preview.CurrentAfter));

                Assert.True(
                    preview.CurrentAfter >= 0d);

                Assert.True(
                    preview.CurrentAfter <=
                    preview.Maximum);

                var entry =
                    ResourceRecoveryOperations.Commit(
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

                Assert.Equal(
                    preview.Result,
                    entry.Result);
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
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));

        var recoveryPreview =
            ResourceRecoveryOperations.Preview(
                state,
                new ResourceRecoveryRequest(
                    state.Id,
                    amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));

        ResourceRecoveryOperations.Commit(
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
                ResourceLossOperations.Commit(
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
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 30d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
            state,
            first);

        var second =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
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
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    amount: 0d, new ResourceOperationProvenance(
        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
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