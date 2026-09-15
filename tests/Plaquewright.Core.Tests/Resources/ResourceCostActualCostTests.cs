using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostActualCostTests
{
    [Fact]
    public void DirectCommit_ActualCostMatchesVisibleStateDelta()
    {
        var setup =
            CreateSetup();

        var currentBefore =
            setup.Target.State.Current;

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.Target.State,
                CreateRequest(
                    setup.ResourceId,
                    amount: 0.1d));

        Assert.True(
            preview.IsPayable);

        var entry =
            ResourceCostOperations.Commit(
                setup.Entity.Id,
                setup.Target.State,
                preview);

        var visibleDelta =
            currentBefore -
            setup.Target.State.Current;

        Assert.Equal(
            0.1d,
            entry.Result.RequestedCost);

        Assert.Equal(
            visibleDelta,
            entry.Result.ActualCost);

        Assert.Equal(
            preview.ProjectedActualCost,
            entry.Result.ActualCost);

        Assert.Equal(
            preview.CurrentAfter,
            setup.Target.State.Current);
    }

    [Fact]
    public void TransactionCommit_ActualCostMatchesVisibleStateDelta()
    {
        var setup =
            CreateSetup();

        var currentBefore =
            setup.Target.State.Current;

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var preview =
            draft.StageCost(
                setup.Target,
                CreateRequest(
                    setup.ResourceId,
                    amount: 0.1d));

        Assert.True(
            preview.IsPayable);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        var entry =
            Assert.IsType<ResourceCostLedgerEntry>(
                Assert.Single(
                    ledger.Entries));

        var visibleDelta =
            currentBefore -
            setup.Target.State.Current;

        Assert.Equal(
            0.1d,
            entry.Result.RequestedCost);

        Assert.Equal(
            visibleDelta,
            entry.Result.ActualCost);

        Assert.Equal(
            preview.ProjectedActualCost,
            entry.Result.ActualCost);

        Assert.Equal(
            preview.CurrentAfter,
            setup.Target.State.Current);
    }

    private static ResourceCostRequest CreateRequest(
        ResourceId resourceId,
        double amount)
    {
        return new ResourceCostRequest(
            resourceId,
            amount,
            new ResourceOperationProvenance(
                ResourceOperationCause.SkillCost));
    }

    private static TestSetup CreateSetup()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var resourceId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        resourceId,
                        current: 100d,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            resourceId,
            entity,
            new ResourceStateTarget(
                entity,
                resourceId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId ResourceId,
        EntityRuntimeState Entity,
        ResourceStateTarget Target);
}