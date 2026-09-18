using System.Diagnostics.CodeAnalysis;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Transactions;
using Plaquewright.Core.Events;

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
    public void CommittedDoorTransaction_ProducesDeterministicFollowUpReaction()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 150d);

        var door =
            new DoorState();

        var alarm =
            new AlarmState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));
        var raiseAlarmReaction =
            new RaiseAlarmReaction();

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var trace =
            new List<string>();

        var waves =
            new List<uint>();

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        var result =
            runner.RunToCompletion(
                context =>
                {
                    waves.Add(
                        context.Key.Wave.Value);

                    switch (context.Payload)
                    {
                        case OpenDoorIntent:
                            {
                                trace.Add(
                                nameof(OpenDoorIntent));

                                var committed =
                                    DomainEventTransactionCoordinator
                                        .TryCommitAndPublish<
                                            TestWorkItem,
                                            DoorOpenedEvent>(
                                            context,
                                            new DoorOpenedEvent(),
                                            payGold,
                                            openDoor);

                                Assert.True(
                                    committed);

                                //
                                // The transaction is committed before the
                                // domain event can execute.
                                //
                                Assert.Equal(
                                    50d,
                                    setup.GoldTarget.State.Current);

                                Assert.Equal(
                                    1UL,
                                    setup.GoldTarget.State.Revision);

                                Assert.Equal(
                                    1,
                                    setup.Ledger.Count);

                                Assert.True(
                                    door.IsOpen);

                                Assert.False(
                                    alarm.IsRaised);
                                break;
                            }

                        case DoorOpenedEvent doorOpened:
                            {
                                trace.Add(
                                    nameof(DoorOpenedEvent));

                                //
                                // Reactions observe already committed state.
                                //
                                Assert.Equal(
                                    50d,
                                    setup.GoldTarget.State.Current);

                                Assert.True(
                                    door.IsOpen);

                                Assert.False(
                                    alarm.IsRaised);

                                raiseAlarmReaction.React(
                                    doorOpened,
                                    new DomainReactionContext<TestWorkItem>(
                                        context));

                                break;
                            }

                        case RaiseAlarmAction:
                            {
                                trace.Add(
                                nameof(RaiseAlarmAction));

                                alarm.Raise();

                                break;
                            }

                        default:
                            throw new InvalidOperationException(
                                "Unknown test work item.");
                    }
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            3UL,
            runner.ProcessedEvents);

        Assert.Equal(
            new[]
            {
            nameof(OpenDoorIntent),
            nameof(DoorOpenedEvent),
            nameof(RaiseAlarmAction)
            },
            trace);

        //
        // Same-time follow-ups advance by one scheduler wave
        // instead of recursing inside the current event.
        //
        Assert.Equal(
            new uint[]
            {
            0u,
            1u,
            2u
            },
            waves);

        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.True(
            door.IsOpen);

        Assert.True(
            alarm.IsRaised);
    }

    [Fact]
    public void RejectedDoorTransaction_ProducesNoDomainEventOrReaction()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 150d);

        var door =
            new DoorState();

        var alarm =
            new AlarmState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: false);

        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var trace =
            new List<string>();

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        var result =
            runner.RunToCompletion(
                context =>
                {
                    trace.Add(
                        context.Payload
                            .GetType()
                            .Name);

                    switch (context.Payload)
                    {
                        case OpenDoorIntent:
                            {
                                var committed =
                                    DomainEventTransactionCoordinator
                                        .TryCommitAndPublish<
                                            TestWorkItem,
                                            DoorOpenedEvent>(
                                            context,
                                            new DoorOpenedEvent(),
                                            payGold,
                                            openDoor);

                                Assert.False(
                                    committed);

                                //
                                // A rejected transaction also cancels the
                                // prepared domain-event publication.
                                //

                                break;
                            }

                        default:
                            throw new InvalidOperationException(
                                "Rejected transaction produced unexpected follow-up work.");
                    }
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            1UL,
            runner.ProcessedEvents);

        Assert.Equal(
            new[]
            {
            nameof(OpenDoorIntent)
            },
            trace);

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

        Assert.False(
            alarm.IsRaised);
    }

    [Fact]
    public void DomainEventReservationFailure_PreventsTransactionCommit()
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

        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 1,
                    maxSameTimestampWave: 10));

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        var result =
    runner.RunNext(
        context =>
        {
            //
            // Occupy the only available queue slot
            // before the transaction/event boundary
            // attempts its reservation.
            //
            context.Schedule(
                new SimulationTime(101L),
                SchedulerPhase.FollowUp,
                new QueueBlockerWorkItem());

            DomainEventTransactionCoordinator
                .TryCommitAndPublish<
                    TestWorkItem,
                    DoorOpenedEvent>(
                    context,
                    new DoorOpenedEvent(),
                    payGold,
                    openDoor);
        });

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            result.Status);

        Assert.Equal(
            SimulationBudgetKind.QueueSize,
            result.BudgetKind);

        //
        // Event publication could not be guaranteed,
        // therefore the transaction was never allowed
        // to become visible.
        //
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
    }

    [Fact]
    public void ReactionFailure_FaultsRunnerWithoutRollingBackCommittedTransaction()
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

        var dispatcher =
            new DomainReactionDispatcher<
                DoorOpenedEvent,
                TestWorkItem>(
                new ThrowingDoorOpenedReaction());

        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        //
        // First event commits Gold + Door and publishes DoorOpened.
        //
        var commitResult =
            runner.RunNext(
                context =>
                {
                    var committed =
                        DomainEventTransactionCoordinator
                            .TryCommitAndPublish<
                                TestWorkItem,
                                DoorOpenedEvent>(
                                context,
                                new DoorOpenedEvent(),
                                payGold,
                                openDoor);

                    Assert.True(
                        committed);
                });

        Assert.Equal(
            SimulationRunStatus.InProgress,
            commitResult.Status);

        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            1,
            setup.Ledger.Count);

        Assert.True(
            door.IsOpen);

        //
        // DoorOpened is already authoritative.
        // Its unexpected reaction failure must fault the runtime,
        // not roll the previous transaction back.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    context =>
                    {
                        var domainEvent =
                            Assert.IsType<DoorOpenedEvent>(
                                context.Payload);

                        dispatcher.Dispatch(
                            domainEvent,
                            new DomainReactionContext<TestWorkItem>(
                                context));
                    }));

        //
        // Original committed state remains authoritative.
        //
        Assert.Equal(
            50d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            1,
            setup.Ledger.Count);

        Assert.True(
            door.IsOpen);

        //
        // Fail-stop: runtime cannot continue.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                    {
                    }));

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.ScheduleExternalInput(
                    new SimulationTime(101L),
                    new OpenDoorIntent()));
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
    public void PayGoldAndOpenDoor_WhenGoldIsAffordableButNotRepresentable_CommitsNeither()
    {
        var setup =
            CreateResourceSetup(
                currentGold: 1e16d,
                maximumGold: 1e16d);

        var door =
            new DoorState();

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 1d);

        //
        // Door deliberately prepares first.
        //
        // Gold is nominally affordable:
        // 1e16 >= 1
        //
        // But subtracting 1 from 1e16 cannot be represented
        // by the current double-based ResourceState.
        //
        var committed =
            TransactionCoordinator.TryCommit(
                openDoor,
                payGold);

        Assert.False(
            committed);

        // Door prepared successfully but must never apply.
        Assert.False(
            door.IsOpen);

        Assert.Equal(
            0,
            door.OpenCount);

        // Gold must remain completely untouched.
        Assert.Equal(
            1e16d,
            setup.GoldTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.GoldTarget.State.Revision);

        Assert.Equal(
            0,
            setup.Ledger.Count);
    }

    [Fact]
    public void DoorEventPipeline_RunNextAndRunToCompletionProduceSameAuthoritativeResult()
    {
        var stepwise =
            RunDoorReactionScenario(
                runToCompletion: false);

        var continuous =
            RunDoorReactionScenario(
                runToCompletion: true);

        Assert.Equal(
            continuous.Trace,
            stepwise.Trace);

        Assert.Equal(
            continuous.Waves,
            stepwise.Waves);

        Assert.Equal(
            continuous.GoldCurrent,
            stepwise.GoldCurrent);

        Assert.Equal(
            continuous.GoldRevision,
            stepwise.GoldRevision);

        Assert.Equal(
            continuous.LedgerCount,
            stepwise.LedgerCount);

        Assert.Equal(
            continuous.DoorIsOpen,
            stepwise.DoorIsOpen);

        Assert.Equal(
            continuous.DoorOpenCount,
            stepwise.DoorOpenCount);

        Assert.Equal(
            continuous.AlarmIsRaised,
            stepwise.AlarmIsRaised);

        Assert.Equal(
            continuous.AlarmRaiseCount,
            stepwise.AlarmRaiseCount);

        Assert.Equal(
            continuous.ProcessedEvents,
            stepwise.ProcessedEvents);

        Assert.Equal(
            new[]
            {
            nameof(OpenDoorIntent),
            nameof(DoorOpenedEvent),
            nameof(RaiseAlarmAction)
            },
            continuous.Trace);

        Assert.Equal(
            new uint[]
            {
            0u,
            1u,
            2u
            },
            continuous.Waves);

        Assert.Equal(
            50d,
            continuous.GoldCurrent);

        Assert.Equal(
            1UL,
            continuous.GoldRevision);

        Assert.Equal(
            1,
            continuous.LedgerCount);

        Assert.True(
            continuous.DoorIsOpen);

        Assert.Equal(
            1,
            continuous.DoorOpenCount);

        Assert.True(
            continuous.AlarmIsRaised);

        Assert.Equal(
            1,
            continuous.AlarmRaiseCount);

        Assert.Equal(
            3UL,
            continuous.ProcessedEvents);
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
    double currentGold,
    double? maximumGold = null)
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
                        maximum: maximumGold ?? 1000d)
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

    private sealed record DoorReactionScenarioResult(
        string[] Trace,
        uint[] Waves,
        double GoldCurrent,
        ulong GoldRevision,
        int LedgerCount,
        bool DoorIsOpen,
        int DoorOpenCount,
        bool AlarmIsRaised,
        int AlarmRaiseCount,
        ulong ProcessedEvents);
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

    private abstract class TestWorkItem
    : ISimulationWorkItem
    {
    }

    private sealed class OpenDoorIntent
        : TestWorkItem
    {
    }

    private sealed class DoorOpenedEvent
    : TestWorkItem,
      IDomainEvent
    {
    }

    private sealed class RaiseAlarmAction
        : TestWorkItem
    {
    }

    private sealed class RaiseAlarmReaction
    : IDomainReaction<
        DoorOpenedEvent,
        TestWorkItem>
    {
        public void React(
            DoorOpenedEvent domainEvent,
            DomainReactionContext<TestWorkItem> context)
        {
            ArgumentNullException.ThrowIfNull(
                domainEvent);

            ArgumentNullException.ThrowIfNull(
                context);

            context.ScheduleFollowUp(
                new RaiseAlarmAction());
        }
    }

    private sealed class AlarmState
    {
        public bool IsRaised { get; private set; }

        public int RaiseCount { get; private set; }

        public void Raise()
        {
            IsRaised = true;
            RaiseCount++;
        }
    }

    private sealed class QueueBlockerWorkItem
    : TestWorkItem
    {
    }

    private sealed class ThrowingDoorOpenedReaction
    : IDomainReaction<
        DoorOpenedEvent,
        TestWorkItem>
    {
        public void React(
            DoorOpenedEvent domainEvent,
            DomainReactionContext<TestWorkItem> context)
        {
            ArgumentNullException.ThrowIfNull(
                domainEvent);

            ArgumentNullException.ThrowIfNull(
                context);

            throw new InvalidOperationException(
                "Synthetic reaction failure.");
        }
    }
    private static DoorReactionScenarioResult
    RunDoorReactionScenario(
        bool runToCompletion)
    {
        var setup =
            CreateResourceSetup(
                currentGold: 150d);

        var door =
            new DoorState();

        var alarm =
            new AlarmState();

        var payGold =
            CreateGoldCostParticipant(
                setup,
                amount: 100d);

        var openDoor =
            new DoorTransactionParticipant(
                door,
                canOpen: true);

        var reaction =
            new RaiseAlarmReaction();

        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var trace =
            new List<string>();

        var waves =
            new List<uint>();

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        void Execute(
            SimulationEventContext<TestWorkItem> context)
        {
            waves.Add(
                context.Key.Wave.Value);

            switch (context.Payload)
            {
                case OpenDoorIntent:
                    {
                        trace.Add(
                            nameof(OpenDoorIntent));

                        var committed =
                            DomainEventTransactionCoordinator
                                .TryCommitAndPublish<
                                    TestWorkItem,
                                    DoorOpenedEvent>(
                                    context,
                                    new DoorOpenedEvent(),
                                    payGold,
                                    openDoor);

                        Assert.True(
                            committed);

                        break;
                    }

                case DoorOpenedEvent doorOpened:
                    {
                        trace.Add(
                            nameof(DoorOpenedEvent));

                        reaction.React(
                            doorOpened,
                            new DomainReactionContext<TestWorkItem>(
                                context));

                        break;
                    }

                case RaiseAlarmAction:
                    {
                        trace.Add(
                            nameof(RaiseAlarmAction));

                        alarm.Raise();

                        break;
                    }

                default:
                    throw new InvalidOperationException(
                        "Unknown test work item.");
            }
        }

        if (runToCompletion)
        {
            var result =
                runner.RunToCompletion(
                    Execute);

            Assert.Equal(
                SimulationRunStatus.Completed,
                result.Status);
        }
        else
        {
            while (true)
            {
                var result =
                    runner.RunNext(
                        Execute);

                if (result.Status ==
                    SimulationRunStatus.Completed)
                {
                    break;
                }

                Assert.Equal(
                    SimulationRunStatus.InProgress,
                    result.Status);
            }
        }

        return new DoorReactionScenarioResult(
            trace.ToArray(),
            waves.ToArray(),
            setup.GoldTarget.State.Current,
            setup.GoldTarget.State.Revision,
            setup.Ledger.Count,
            door.IsOpen,
            door.OpenCount,
            alarm.IsRaised,
            alarm.RaiseCount,
            runner.ProcessedEvents);
    }

}