using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceLossCommitOwnershipTests
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
            new EntityId(42UL),
            entry.TargetEntityId);

        Assert.Equal(
            lifeId,
            entry.ResourceId);

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
            firstTarget.State.Current);

        Assert.Equal(
            100d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }
}