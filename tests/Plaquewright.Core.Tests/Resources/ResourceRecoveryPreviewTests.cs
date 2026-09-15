using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

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
    public void PreviewRecovery_WhenRequestedRecoveryIsNotRepresentable_DoesNotReportPhantomActualRecovery()
    {
        var current =
            1e16d;

        var state =
            CreateState(
                current: current,
                maximum: current + 10d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 1d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            current,
            preview.CurrentBefore);

        Assert.Equal(
            current,
            preview.CurrentAfter);

        Assert.Equal(
            0d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            1d,
            preview.Result.Overflow);

        Assert.Equal(
            preview.CurrentAfter -
            preview.CurrentBefore,
            preview.Result.ActualRecovery);
    }

    [Fact]
    public void PreviewRecovery_WhenRoundedAdditionWouldOverrecover_UsesConservativeRepresentableRecovery()
    {
        var current =
            1e16d;

        var state =
            CreateState(
                current: current,
                maximum: current + 10d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 3d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            2d,
            preview.Result.ActualRecovery);

        Assert.Equal(
            1d,
            preview.Result.Overflow);

        Assert.Equal(
            current + 2d,
            preview.CurrentAfter);

        Assert.Equal(
            preview.CurrentAfter -
            preview.CurrentBefore,
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
                    current: 25d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceRecoveryRequest(
                lifeId,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
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
            50d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            0d,
            entry.Result.Overflow);
    }

    [Fact]
    public void CommitRecovery_RejectsStalePreview()
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
                    current: 25d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var stalePreview =
            ResourceRecoveryOperations.Preview(
                target.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    amount: 50d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        var otherPreview =
            ResourceRecoveryOperations.Preview(
                target.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    amount: 10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        ResourceRecoveryOperations.Commit(
            target,
            otherPreview);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    target,
                    stalePreview));

        Assert.Equal(
            35d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);
    }

    [Fact]
    public void CommitRecovery_RejectsDifferentStateWithSameResourceId()
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
                    current: 25d,
                    maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 25d,
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
            ResourceRecoveryOperations.Preview(
                firstTarget.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    amount: 50d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            25d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }

    [Fact]
    public void ZeroRecoveryCommit_IsStillExplicitCommit()
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
                    current: 25d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceRecoveryRequest(
                lifeId,
                amount: 0d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                target,
                preview);

        Assert.Equal(
            25d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

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