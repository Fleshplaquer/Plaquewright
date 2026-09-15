using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostCommitTests
{
    [Fact]
    public void AffordableCost_CommitsFullAmount()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                setup.Registry,
                [
                    setup.State
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                setup.State.Id);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                target.State,
                new ResourceCostRequest(
                    target.State.Id,
                    amount: 40d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var entry =
            ResourceCostOperations.Commit(
                target,
                preview);

        Assert.Equal(
            60d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Equal(
            40d,
            entry.Result.RequestedCost);

        Assert.Equal(
            40d,
            entry.Result.ActualCost);
    }

    [Fact]
    public void UnaffordableCost_CannotBeCommitted()
    {
        var setup =
            CreateSetup(
                current: 50d);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                setup.Registry,
                [
                    setup.State
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                setup.State.Id);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                target.State,
                new ResourceCostRequest(
                    target.State.Id,
                    amount: 100d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    target,
                    preview));

        Assert.Equal(
            50d,
            target.State.Current);

        Assert.Equal(
            0UL,
            target.State.Revision);
    }

    [Fact]
    public void Commit_RejectsStalePreview()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                setup.Registry,
                [
                    setup.State
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                setup.State.Id);

        var stale =
            ResourceCostOperations.Preview(
                setup.Registry,
                target.State,
                new ResourceCostRequest(
                    target.State.Id,
                    amount: 20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var newer =
            ResourceCostOperations.Preview(
                setup.Registry,
                target.State,
                new ResourceCostRequest(
                    target.State.Id,
                    amount: 10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        ResourceCostOperations.Commit(
            target,
            newer);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    target,
                    stale));

        Assert.Equal(
            90d,
            target.State.Current);
    }

    [Fact]
    public void Commit_RejectsDifferentStateWithSameResourceId()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                setup.Registry,
                [
                    setup.State
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                setup.State.Id);

        var otherState =
            new ResourceState(
                setup.State.Id,
                current: 100d,
                maximum: 100d);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                setup.Registry,
                [
                    otherState
                ]);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                otherState.Id);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                firstTarget.State,
                new ResourceCostRequest(
                    firstTarget.State.Id,
                    amount: 20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
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
    public void ZeroCostCommit_IsStillExplicitCommit()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                setup.Registry,
                [
                    setup.State
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                setup.State.Id);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                target.State,
                new ResourceCostRequest(
                    target.State.Id,
                    amount: 0d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var entry =
            ResourceCostOperations.Commit(
                target,
                preview);

        Assert.Equal(
            100d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Equal(
            0d,
            entry.Result.ActualCost);
    }

    private static (
        CompiledResourceRegistry Registry,
        ResourceState State)
        CreateSetup(
            double current)
    {
        var key =
            ResourceKey.Parse(
                "resource.mana");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    key,
                    ResourceRole.CostSource)
            ]);

        var state =
            new ResourceState(
                registry.GetId(key),
                current,
                maximum: 100d);

        return (
            registry,
            state);
    }
}