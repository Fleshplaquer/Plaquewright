using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostCommitOwnershipTests
{
    [Fact]
    public void Commit_DerivesLedgerOwnerFromResourceTarget()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var entity =
            new EntityRuntimeState(
                new EntityId(42UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                target.State,
                new ResourceCostRequest(
                    manaId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var entry =
            ResourceCostOperations.Commit(
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

        Assert.Equal(
            25d,
            entry.Result.ActualCost);
    }

    [Fact]
    public void Commit_WithTargetForDifferentState_RejectsBeforeMutation()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                manaId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                manaId);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                firstTarget.State,
                new ResourceCostRequest(
                    manaId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            100d,
            firstTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            100d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }
}