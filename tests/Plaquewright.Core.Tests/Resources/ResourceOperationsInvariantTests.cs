using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationsInvariantTests
{




    [Fact]
    public void RepresentativeLossOperations_PreserveAllInvariants()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

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
                    var entity =
                        new EntityRuntimeState(
                            new EntityId(1UL),
                            registry,
                            [
                                new ResourceState(
                                lifeId,
                                current,
                                maximum: 100d)
                            ]);

                    var target =
                        new ResourceStateTarget(
                            entity,
                            lifeId);

                    var request =
                        new ResourceLossRequest(
                            lifeId,
                            requested,
                            new ResourceOperationProvenance(
                                ResourceOperationCause.DamageDerived));

                    var preview =
                        ResourceLossOperations.Preview(
                            target.State,
                            request,
                            prevented);

                    // Preview must not mutate state.
                    Assert.Equal(
                        current,
                        target.State.Current);

                    Assert.Equal(
                        0UL,
                        target.State.Revision);

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
                            target,
                            preview);

                    Assert.Equal(
                        preview.CurrentAfter,
                        target.State.Current);

                    Assert.Equal(
                        preview.Maximum,
                        target.State.Maximum);

                    Assert.Equal(
                        1UL,
                        target.State.Revision);

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
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

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
                var entity =
                    new EntityRuntimeState(
                        new EntityId(1UL),
                        registry,
                        [
                            new ResourceState(
                            lifeId,
                            current,
                            maximum: 100d)
                        ]);

                var target =
                    new ResourceStateTarget(
                        entity,
                        lifeId);

                var request =
                    new ResourceRecoveryRequest(
                        lifeId,
                        requested,
                        new ResourceOperationProvenance(
                            ResourceOperationCause.DamageDerived));

                var preview =
                    ResourceRecoveryOperations.Preview(
                        target.State,
                        request);

                // Preview must not mutate state.
                Assert.Equal(
                    current,
                    target.State.Current);

                Assert.Equal(
                    0UL,
                    target.State.Revision);

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
                        target,
                        preview);

                Assert.Equal(
                    preview.CurrentAfter,
                    target.State.Current);

                Assert.Equal(
                    preview.Maximum,
                    target.State.Maximum);

                Assert.Equal(
                    1UL,
                    target.State.Revision);

                Assert.Equal(
                    preview.Result,
                    entry.Result);
            }
        }
    }

    [Fact]
    public void CommitMakesEveryOlderPreviewStaleRegardlessOfOperationType()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var lossPreview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.DamageDerived)));

        var recoveryPreview =
            ResourceRecoveryOperations.Preview(
                target.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    amount: 20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.DamageDerived)));

        ResourceRecoveryOperations.Commit(
            target,
            recoveryPreview);

        Assert.Equal(
            70d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    target,
                    lossPreview));

        Assert.Equal(
            70d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);
    }

    [Fact]
    public void SequentialOperations_RequireFreshPreview()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var first =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
            target,
            first);

        var second =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
            target,
            second);

        Assert.Equal(
            50d,
            target.State.Current);

        Assert.Equal(
            2UL,
            target.State.Revision);
    }

    [Fact]
    public void ZeroAmountCommit_IsStillAnExplicitStateCommit()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 0d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.DamageDerived)));

        ResourceLossOperations.Commit(
            target,
            preview);

        Assert.Equal(
            50d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);
    }

}