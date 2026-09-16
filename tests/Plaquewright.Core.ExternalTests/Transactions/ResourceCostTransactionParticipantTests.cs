using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Core.ExternalTests.Transactions;

public sealed class ResourceCostTransactionParticipantTests
{
    [Fact]
    public void AffordableCost_CommitsThroughTransactionCoordinator()
    {
        var setup =
            CreateSetup(
                currentGold: 150d);

        var participant =
            new ResourceCostTransactionParticipant(
                setup.GoldTarget,
                new ResourceCostRequest(
                    setup.GoldId,
                    100d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)),
                setup.Ledger);

        var committed =
            TransactionCoordinator.TryCommit(
                participant);

        Assert.True(
            committed);

        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            1,
            setup.Ledger.Count);

        var entry =
            Assert.IsType<ResourceCostLedgerEntry>(
                setup.Ledger.Entries[0]);

        Assert.Equal(
            100d,
            entry.Result.RequestedCost);

        Assert.Equal(
            100d,
            entry.Result.ActualCost);
    }

    [Fact]
    public void UnaffordableCost_RejectsWithoutPublishingAnything()
    {
        var setup =
            CreateSetup(
                currentGold: 50d);

        var participant =
            new ResourceCostTransactionParticipant(
                setup.GoldTarget,
                new ResourceCostRequest(
                    setup.GoldId,
                    100d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)),
                setup.Ledger);

        var committed =
            TransactionCoordinator.TryCommit(
                participant);

        Assert.False(
            committed);

        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            0,
            setup.Ledger.Count);
    }

    [Fact]
    public void TryPrepare_DoesNotPublishAndReturnsNoPublicPublicationCapability()
    {
        var setup =
            CreateSetup(
                currentGold: 150d);

        var participant =
            new ResourceCostTransactionParticipant(
                setup.GoldTarget,
                new ResourceCostRequest(
                    setup.GoldId,
                    100d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)),
                setup.Ledger);

        var accepted =
            participant.TryPrepare(
                out var preparedChange);

        Assert.True(
            accepted);

        Assert.NotNull(
            preparedChange);

        // Preparation itself must not make gameplay state visible.
        Assert.Equal(
            150d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            0,
            setup.Ledger.Count);

        var publicPublicationMethods =
            preparedChange
                .GetType()
                .GetMethods(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public)
                .Where(
                    method =>
                        method.Name is
                            "Apply" or
                            "Commit" or
                            "Publish")
                .ToArray();

        Assert.Empty(
            publicPublicationMethods);
    }

    private static TestSetup CreateSetup(
        double currentGold)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.gold"),
                    ResourceRole.CostSource)
            ]);

        var goldId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.gold"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        goldId,
                        currentGold,
                        maximum: 1000d)
                ]);

        var goldTarget =
            new ResourceStateTarget(
                entity,
                goldId);

        return new TestSetup(
            goldId,
            goldTarget,
            new ResourceOperationLedger());
    }

    private sealed record TestSetup(
        ResourceId GoldId,
        ResourceStateTarget GoldTarget,
        ResourceOperationLedger Ledger);
}