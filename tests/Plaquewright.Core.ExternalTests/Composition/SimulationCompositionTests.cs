using Plaquewright.Core.Composition;
using Plaquewright.Core.Events;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.ExternalTests.Composition;

public sealed class SimulationCompositionTests
{
    [Fact]
    public void DoorAndAlarmModules_ComposeAndRunHeadless()
    {
        var door =
            new DoorState();

        var alarm =
            new AlarmState();

        var trace =
            new List<string>();

        var alarmReaction =
            new RaiseAlarmReaction();

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

                module.Handle<DoorOpenedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(DoorOpenedEvent));

                        alarmReaction.React(
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

        var result =
            runner.RunToCompletion(
                composition.Execute);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            2,
            composition.ModuleCount);

        Assert.Equal(
            3,
            composition.HandlerCount);

        Assert.True(
            composition.Provides<IDoorContract>());

        Assert.True(
            composition.Provides<IAlarmContract>());

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
    public void Build_WhenRequiredContractIsMissing_FailsBeforeSimulationStarts()
    {
        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "Alarm",
            module =>
            {
                module.Requires<IDoorContract>();

                module.Provides<IAlarmContract>();
            });

        Assert.Throws<InvalidOperationException>(
            () =>
                builder.Build());
    }

    [Fact]
    public void Build_WhenTwoModulesProvideSameContract_IsRejected()
    {
        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "FirstDoor",
            module =>
                module.Provides<IDoorContract>());

        builder.AddModule(
            "SecondDoor",
            module =>
                module.Provides<IDoorContract>());

        Assert.Throws<InvalidOperationException>(
            () =>
                builder.Build());
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