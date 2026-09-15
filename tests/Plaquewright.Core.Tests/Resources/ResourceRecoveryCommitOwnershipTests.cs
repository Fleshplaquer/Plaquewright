using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceRecoveryCommitOwnershipTests
{
    [Fact]
    public void Commit_DerivesLedgerOwnerFromResourceTarget()
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
                new EntityId(42UL),
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
            ResourceRecoveryOperations.Preview(
                target.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        var entry =
            ResourceRecoveryOperations.Commit(
                target,
                preview);

        Assert.Equal(
            new EntityId(42UL),
            entry.TargetEntityId);

        Assert.Equal(
            75d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);
    }

    [Fact]
    public void Commit_WithTargetForDifferentState_RejectsBeforeMutation()
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
                        current: 50d,
                        maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 50d,
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
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            50d,
            firstTarget.State.Current);

        Assert.Equal(
            50d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }
}