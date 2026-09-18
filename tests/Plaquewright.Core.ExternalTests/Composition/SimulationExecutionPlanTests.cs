using Plaquewright.Core.Composition;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.ExternalTests.Composition;

public sealed class SimulationExecutionPlanTests
{
    [Fact]
    public void ExecutionPlan_RoutesWorkInDeterministicSchedulerOrder()
    {
        var builder =
            new SimulationExecutionPlanBuilder<TestWorkItem>();

        var trace =
            new List<string>();

        builder.Handle<OpenDoorIntent>(
            (intent, context) =>
            {
                ArgumentNullException.ThrowIfNull(
                    intent);

                trace.Add(
                    nameof(OpenDoorIntent));

                context.Schedule(
                    context.CurrentTime,
                    SchedulerPhase.FollowUp,
                    new DoorOpenedEvent());
            });

        builder.Handle<DoorOpenedEvent>(
            (domainEvent, context) =>
            {
                ArgumentNullException.ThrowIfNull(
                    domainEvent);

                trace.Add(
                    nameof(DoorOpenedEvent));

                context.Schedule(
                    context.CurrentTime,
                    SchedulerPhase.FollowUp,
                    new RaiseAlarmAction());
            });

        builder.Handle<RaiseAlarmAction>(
            (action, _) =>
            {
                ArgumentNullException.ThrowIfNull(
                    action);

                trace.Add(
                    nameof(RaiseAlarmAction));
            });

        var plan =
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
                plan.Execute);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            3,
            plan.HandlerCount);

        Assert.Equal(
            new[]
            {
                nameof(OpenDoorIntent),
                nameof(DoorOpenedEvent),
                nameof(RaiseAlarmAction)
            },
            trace);
    }

    [Fact]
    public void ExecutionPlan_DuplicateExactHandler_IsRejectedDuringComposition()
    {
        var builder =
            new SimulationExecutionPlanBuilder<TestWorkItem>();

        builder.Handle<OpenDoorIntent>(
            (_, _) =>
            {
            });

        Assert.Throws<InvalidOperationException>(
            () =>
                builder.Handle<OpenDoorIntent>(
                    (_, _) =>
                    {
                    }));
    }

    [Fact]
    public void BuiltExecutionPlan_IsNotChangedByLaterBuilderRegistrations()
    {
        var builder =
            new SimulationExecutionPlanBuilder<TestWorkItem>();

        builder.Handle<OpenDoorIntent>(
            (_, _) =>
            {
            });

        var firstPlan =
            builder.Build();

        builder.Handle<DoorOpenedEvent>(
            (_, _) =>
            {
            });

        var secondPlan =
            builder.Build();

        Assert.Equal(
            1,
            firstPlan.HandlerCount);

        Assert.Equal(
            2,
            secondPlan.HandlerCount);
    }

    private abstract class TestWorkItem
    {
    }

    private sealed class OpenDoorIntent
        : TestWorkItem
    {
    }

    private sealed class DoorOpenedEvent
        : TestWorkItem
    {
    }

    private sealed class RaiseAlarmAction
        : TestWorkItem
    {
    }
}