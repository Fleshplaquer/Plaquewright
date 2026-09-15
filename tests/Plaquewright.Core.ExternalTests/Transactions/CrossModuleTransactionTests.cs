using System.Diagnostics.CodeAnalysis;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Core.ExternalTests.Transactions;

public sealed class CrossModuleTransactionTests
{
    [Fact]
    public void PayGoldAndOpenDoor_WhenBothCanPrepare_CommitsBoth()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 150d);

        var door =
            new DoorState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var committed =
            TransactionCoordinator.TryCommit(
                payGold,
                openDoor);

        Assert.True(
            committed);

        // Resource module committed.
        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            1,
            setup.Ledger.Count);

        var costEntry =
            Assert.IsType<ResourceCostLedgerEntry>(
                setup.Ledger.Entries[0]);

        Assert.Equal(
            100d,
            costEntry.Result.RequestedCost);

        Assert.Equal(
            100d,
            costEntry.Result.ActualCost);

        // Independent Door module committed.
        Assert.True(
            door.IsOpen);

        Assert.Equal(
            1,
            door.OpenCount);
    }

    [Fact]
    public void PayGoldAndOpenDoor_WhenDoorRejects_CommitsNeither()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 150d);

        var door =
            new DoorState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: false);

        var committed =
            TransactionCoordinator.TryCommit(
                payGold,
                openDoor);

        Assert.False(
            committed);

        // Gold participant prepared successfully first,
        // but Door rejected afterwards.
        //
        // The prepared Resource transaction must therefore
        // never become visible.
        Assert.Equal(
            150d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            0,
            setup.Ledger.Count);

        Assert.False(
            door.IsOpen);

        Assert.Equal(
            0,
            door.OpenCount);
    }

    [Fact]
    public void PayGoldAndOpenDoor_WhenGoldRejects_CommitsNeither()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 50d);

        var door =
            new DoorState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var committed =
            TransactionCoordinator.TryCommit(
                payGold,
                openDoor);

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

        Assert.False(
            door.IsOpen);

        Assert.Equal(
            0,
            door.OpenCount);
    }

    [Fact]
    public void PayGoldAndOpenDoor_WhenDoorPreparesBeforeGoldRejects_CommitsNeither()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 50d);

        var door =
            new DoorState();

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        //
        // Reverse participant order deliberately.
        //
        // Door prepares successfully first.
        // Gold then rejects.
        //
        var committed =
            TransactionCoordinator.TryCommit(
                openDoor,
                payGold);

        Assert.False(
            committed);

        Assert.False(
            door.IsOpen);

        Assert.Equal(
            0,
            door.OpenCount);

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

    private static ResourceCostTransactionParticipant
        CreateGoldCostParticipant(
            ResourceSetup setup,
            double amount)
    {
        return new ResourceCostTransactionParticipant(
            setup.GoldTarget,
            new ResourceCostRequest(
                setup.GoldId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct)),
            setup.Ledger);
    }

    private static ResourceSetup CreateResourceSetup(
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

        return new ResourceSetup(
            goldId,
            goldTarget,
            new ResourceOperationLedger());
    }

    private sealed record ResourceSetup(
        ResourceId GoldId,
        ResourceStateTarget GoldTarget,
        ResourceOperationLedger Ledger);

    //
    // Synthetic external module.
    //
    // This deliberately does not live in Plaquewright.Core.
    //
    private sealed class DoorState
    {
        public bool IsOpen { get; private set; }

        public int OpenCount { get; private set; }

        public void Open()
        {
            IsOpen =
                true;

            OpenCount++;
        }
    }

    private sealed class DoorTransactionParticipant
        : ITransactionParticipant
    {
        private readonly DoorState _door;

        private readonly bool _canOpen;

        public DoorTransactionParticipant(
            DoorState door,
            bool canOpen)
        {
            ArgumentNullException.ThrowIfNull(
                door);

            _door =
                door;

            _canOpen =
                canOpen;
        }

        public bool TryPrepare(
            [NotNullWhen(true)]
            out PreparedTransactionChange? preparedChange)
        {
            if (!_canOpen ||
                _door.IsOpen)
            {
                preparedChange =
                    null;

                return false;
            }

            preparedChange =
                new PreparedDoorOpen(
                    _door);

            return true;
        }

        private sealed class PreparedDoorOpen
            : PreparedTransactionChange
        {
            private readonly DoorState _door;

            public PreparedDoorOpen(
                DoorState door)
            {
                _door =
                    door;
            }

            protected override void ApplyCore()
            {
                _door.Open();
            }
        }
    }
}