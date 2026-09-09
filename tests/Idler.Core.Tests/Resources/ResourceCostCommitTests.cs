using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceCostCommitTests
{
    [Fact]
    public void AffordableCost_CommitsFullAmount()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
    setup.State.Id,
    amount: 40d,
    new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        var entry =
            ResourceCostOperations.Commit(
                new EntityId(1UL),
                setup.State,
                preview);

        Assert.Equal(
            60d,
            setup.State.Current);

        Assert.Equal(
            1UL,
            setup.State.Revision);

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

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    new EntityId(1UL),
                    setup.State,
                    preview));

        Assert.Equal(
            50d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void Commit_RejectsStalePreview()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var stale =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        var newer =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        ResourceCostOperations.Commit(
            new EntityId(1UL),
            setup.State,
            newer);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    new EntityId(1UL),
                    setup.State,
                    stale));

        Assert.Equal(
            90d,
            setup.State.Current);
    }

    [Fact]
    public void Commit_RejectsDifferentStateWithSameResourceId()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var otherState =
            new ResourceState(
                setup.State.Id,
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 20d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    new EntityId(1UL),
                    otherState,
                    preview));

        Assert.Equal(
            100d,
            otherState.Current);
    }

    [Fact]
    public void ZeroCostCommit_IsStillExplicitCommit()
    {
        var setup =
            CreateSetup(
                current: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 0d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        var entry =
            ResourceCostOperations.Commit(
                new EntityId(1UL),
                setup.State,
                preview);

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            1UL,
            setup.State.Revision);

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