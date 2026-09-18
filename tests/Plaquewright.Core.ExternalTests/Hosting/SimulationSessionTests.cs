using Plaquewright.Core.Composition;
using Plaquewright.Core.Events;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;

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

    private sealed class DoorState
    {
        public bool IsOpen { get; private set; }

        public void Open()
        {
            IsOpen =
                true;
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