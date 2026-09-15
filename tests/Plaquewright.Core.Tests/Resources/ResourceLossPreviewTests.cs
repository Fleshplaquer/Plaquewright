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
    public void Preview_WhenRequestedLossIsNotRepresentable_DoesNotReportPhantomActualLoss()
    {
        var resourceId =
            new ResourceId(1);

        var state =
            new ResourceState(
                resourceId,
                current: 1e16d,
                maximum: 1e16d);

        var request =
            new ResourceLossRequest(
                resourceId,
                amount: 1d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        Assert.Equal(
            1e16d,
            preview.CurrentBefore);

        Assert.Equal(
            1e16d,
            preview.CurrentAfter);

        Assert.Equal(
            0d,
            preview.Result.ActualLoss);

        Assert.Equal(
            1d,
            preview.Result.Shortfall);

        Assert.Equal(
            preview.CurrentBefore -
            preview.CurrentAfter,
            preview.Result.ActualLoss);
    }

    [Fact]
    public void Preview_WhenRoundedSubtractionWouldOvercharge_UsesConservativeRepresentableLoss()
    {
        var resourceId =
            new ResourceId(1);

        var state =
            new ResourceState(
                resourceId,
                current: 1e16d,
                maximum: 1e16d);

        var request =
            new ResourceLossRequest(
                resourceId,
                amount: 3d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        Assert.Equal(
            2d,
            preview.Result.ActualLoss);

        Assert.Equal(
            1d,
            preview.Result.Shortfall);

        Assert.Equal(
            1e16d - 2d,
            preview.CurrentAfter);

        Assert.Equal(
            preview.CurrentBefore -
            preview.CurrentAfter,
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

        var request =
            new ResourceLossRequest(
                lifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceLossOperations.Commit(
                target,
                preview);

        Assert.Equal(
            75d,
            target.State.Current);

        Assert.Equal(
            100d,
            target.State.Maximum);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Equal(
            25d,
            entry.Result.ActualLoss);
    }

    [Fact]
    public void CommitLoss_RejectsStalePreview()
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

        var stalePreview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        var otherPreview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        ResourceLossOperations.Commit(
            target,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    target,
                    stalePreview));

        Assert.Equal(
            90d,
            target.State.Current);
    }

    [Fact]
    public void CommitLoss_RejectsDifferentStateWithSameResourceId()
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

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var preview =
            ResourceLossOperations.Preview(
                firstTarget.State,
                new ResourceLossRequest(
                    lifeId,
                    amount: 25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            100d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }

    [Fact]
    public void ZeroLossCommit_IsStillExplicitCommit()
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
                        ResourceOperationCause.Direct)));

        var entry =
            ResourceLossOperations.Commit(
                target,
                preview);

        Assert.Equal(
            50d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

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