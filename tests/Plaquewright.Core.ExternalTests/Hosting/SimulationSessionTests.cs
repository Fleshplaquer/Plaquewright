using Plaquewright.Core.Composition;
using Plaquewright.Core.Events;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;
using System.Diagnostics.CodeAnalysis;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Core.ExternalTests.Hosting;

public sealed class SimulationSessionTests
{
    [Fact]
    public void Session_RunsComposedGameplayHeadlessWithoutHostDispatchLogic()
    {
        var door =
            new DoorState();

        var alarm =
            new AlarmState();

        var trace =
            new List<string>();

        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "Door",
            module =>
            {
                module.Provides<IDoorContract>();

                module.Handle<OpenDoorIntent>(
                    (_, context) =>
                    {
                        trace.Add(
                            nameof(OpenDoorIntent));

                        door.Open();

                        context.Schedule(
                            context.CurrentTime,
                            SchedulerPhase.FollowUp,
                            new DoorOpenedEvent());
                    });
            });

        builder.AddModule(
            "Alarm",
            module =>
            {
                module.Requires<IDoorContract>();

                module.Provides<IAlarmContract>();

                var reaction =
                    new RaiseAlarmReaction();

                module.Handle<DoorOpenedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(DoorOpenedEvent));

                        reaction.React(
                            domainEvent,
                            new DomainReactionContext<TestWorkItem>(
                                context));
                    });

                module.Handle<RaiseAlarmAction>(
                    (_, _) =>
                    {
                        trace.Add(
                            nameof(RaiseAlarmAction));

                        alarm.Raise();
                    });
            });

        var composition =
            builder.Build();

        var session =
            new SimulationSession<TestWorkItem>(
                composition,
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        session.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        var result =
            session.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            new SimulationTime(100L),
            session.CurrentTime);

        Assert.Equal(
            3UL,
            session.ProcessedEvents);

        Assert.Equal(
            new[]
            {
                nameof(OpenDoorIntent),
                nameof(DoorOpenedEvent),
                nameof(RaiseAlarmAction)
            },
            trace);

        Assert.True(
            door.IsOpen);

        Assert.True(
            alarm.IsRaised);
    }

    [Fact]
    public void Session_ComposesResourcesDoorAndAlarmForPaidOpeningWithoutCombat()
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
                door);

        var reaction =
            new RaiseAlarmReaction();

        var trace =
            new List<string>();

        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "Resources",
            module =>
            {
                module.Provides<IResourcesContract>();
            });

        builder.AddModule(
            "Door",
            module =>
            {
                module.Requires<IResourcesContract>();
                module.Provides<IDoorContract>();

                module.Handle<OpenDoorIntent>(
                    (_, context) =>
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
                    });
            });

        builder.AddModule(
            "Alarm",
            module =>
            {
                module.Requires<IDoorContract>();
                module.Provides<IAlarmContract>();

                module.Handle<DoorOpenedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(DoorOpenedEvent));

                        reaction.React(
                            domainEvent,
                            new DomainReactionContext<TestWorkItem>(
                                context));
                    });

                module.Handle<RaiseAlarmAction>(
                    (_, _) =>
                    {
                        trace.Add(
                            nameof(RaiseAlarmAction));

                        alarm.Raise();
                    });
            });

        var composition =
            builder.Build();

        var session =
            CreateSession(
                composition);

        session.ScheduleExternalInput(
            new SimulationTime(100L),
            new OpenDoorIntent());

        var result =
            session.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            3,
            composition.ModuleCount);

        Assert.True(
            composition.Provides<IResourcesContract>());

        Assert.True(
            composition.Provides<IDoorContract>());

        Assert.True(
            composition.Provides<IAlarmContract>());

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

        Assert.True(
            alarm.IsRaised);

        Assert.Equal(
            new[]
            {
            nameof(OpenDoorIntent),
            nameof(DoorOpenedEvent),
            nameof(RaiseAlarmAction)
            },
            trace);

        Assert.Equal(
            3UL,
            session.ProcessedEvents);
    }

    [Fact]
    public void Session_RunNextUsesSameCompositionAcrossSteps()
    {
        var executed =
            new List<string>();

        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "Test",
            module =>
            {
                module.Handle<FirstWorkItem>(
                    (_, context) =>
                    {
                        executed.Add(
                            nameof(FirstWorkItem));

                        context.Schedule(
                            context.CurrentTime,
                            SchedulerPhase.FollowUp,
                            new SecondWorkItem());
                    });

                module.Handle<SecondWorkItem>(
                    (_, _) =>
                    {
                        executed.Add(
                            nameof(SecondWorkItem));
                    });
            });

        var session =
            CreateSession(
                builder.Build());

        session.ScheduleExternalInput(
            new SimulationTime(25L),
            new FirstWorkItem());

        var first =
            session.RunNext();

        Assert.Equal(
            SimulationRunStatus.InProgress,
            first.Status);

        Assert.Equal(
            1UL,
            session.ProcessedEvents);

        var second =
            session.RunNext();

        Assert.Equal(
            SimulationRunStatus.Completed,
            second.Status);

        Assert.Equal(
            2UL,
            session.ProcessedEvents);

        Assert.Equal(
            new[]
            {
                nameof(FirstWorkItem),
                nameof(SecondWorkItem)
            },
            executed);
    }

    [Fact]
    public void Session_PreservesRunnerExternalInputBoundary()
    {
        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "Test",
            module =>
            {
                module.Handle<FirstWorkItem>(
                    (_, _) =>
                    {
                    });
            });

        var session =
            CreateSession(
                builder.Build());

        session.ScheduleExternalInput(
            new SimulationTime(100L),
            new FirstWorkItem());

        var result =
            session.RunNext();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Throws<InvalidOperationException>(
            () =>
                session.ScheduleExternalInput(
                    new SimulationTime(100L),
                    new FirstWorkItem()));

        session.ScheduleExternalInput(
            new SimulationTime(101L),
            new FirstWorkItem());

        var future =
            session.RunNext();

        Assert.Equal(
            SimulationRunStatus.Completed,
            future.Status);

        Assert.Equal(
            new SimulationTime(101L),
            session.CurrentTime);
    }

    private static SimulationSession<TestWorkItem>
        CreateSession(
            SimulationComposition<TestWorkItem> composition)
    {
        return new SimulationSession<TestWorkItem>(
            composition,
            new SimulationSchedulerLimits(
                maxQueueSize: 100,
                maxSameTimestampWave: 10),
            new SimulationRunnerLimits(
                maxProcessedEvents: 100UL));
    }

    private interface IDoorContract
        : ISimulationModuleContract
    {
    }

    private interface IAlarmContract
        : ISimulationModuleContract
    {
    }

    private abstract class TestWorkItem
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

    private sealed class FirstWorkItem
        : TestWorkItem
    {
    }

    private sealed class SecondWorkItem
        : TestWorkItem
    {
    }
    private interface IResourcesContract
    : ISimulationModuleContract
    {
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

    private sealed class DoorState
    {
        public bool IsOpen { get; private set; }

        public void Open()
        {
            IsOpen =
                true;
        }
    }

    private sealed class DoorTransactionParticipant
    : ITransactionParticipant
    {
        private readonly DoorState _door;

        public DoorTransactionParticipant(
            DoorState door)
        {
            ArgumentNullException.ThrowIfNull(
                door);

            _door =
                door;
        }

        public bool TryPrepare(
            [NotNullWhen(true)]
        out PreparedTransactionChange? preparedChange)
        {
            if (_door.IsOpen)
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

    private sealed class AlarmState
    {
        public bool IsRaised { get; private set; }

        public void Raise()
        {
            IsRaised =
                true;
        }
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
}