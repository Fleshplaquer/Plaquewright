using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationEventContextTests
{
    [Fact]
    public void Context_ExposesCurrentEventData()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var time =
            new SimulationTime(
                10);

        var expectedKey =
            scheduler.Schedule(
                time,
                SchedulerPhase.Execution,
                "parent");

        var runner =
            CreateRunner(
                scheduler);

        ScheduledEventKey actualKey = default;
        SimulationTime actualTime = default;
        string? actualPayload = null;

        runner.RunNext(
            context =>
            {
                actualKey =
                    context.Key;

                actualTime =
                    context.CurrentTime;

                actualPayload =
                    context.Payload;
            });

        Assert.Equal(
            expectedKey,
            actualKey);

        Assert.Equal(
            time,
            actualTime);

        Assert.Equal(
            "parent",
            actualPayload);
    }

    [Fact]
    public void Schedule_AtSameTime_UsesNextWave()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var time =
            new SimulationTime(
                10);

        var parentKey =
            scheduler.Schedule(
                time,
                SchedulerPhase.Execution,
                "parent");

        var runner =
            CreateRunner(
                scheduler);

        runner.RunNext(
            context =>
            {
                context.Schedule(
                    context.CurrentTime,
                    SchedulerPhase.FollowUp,
                    "child");
            });

        Assert.True(
            scheduler.TryPeek(
                out var child));

        Assert.Equal(
            parentKey.Wave.Next(),
            child.Key.Wave);

        Assert.Equal(
            "child",
            child.Payload);
    }

    [Fact]
    public void Schedule_InFuture_UsesInitialWave()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(10),
            SchedulerPhase.Execution,
            "parent");

        var runner =
            CreateRunner(
                scheduler);

        runner.RunNext(
            context =>
            {
                context.Schedule(
                    new SimulationTime(20),
                    SchedulerPhase.FollowUp,
                    "child");
            });

        Assert.True(
            scheduler.TryPeek(
                out var child));

        Assert.Equal(
            SchedulerWave.Initial,
            child.Key.Wave);
    }

    [Fact]
    public void RetainedContext_AfterHandlerReturns_CannotSchedule()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(10),
            SchedulerPhase.Execution,
            "parent");

        var runner =
            CreateRunner(
                scheduler);

        SimulationEventContext<string>? retainedContext =
            null;

        runner.RunNext(
            context =>
            {
                retainedContext =
                    context;
            });

        Assert.NotNull(
            retainedContext);

        Assert.Throws<InvalidOperationException>(
            () =>
                retainedContext!.Schedule(
                    new SimulationTime(20),
                    SchedulerPhase.FollowUp,
                    "late-child"));
    }

    [Fact]
    public void RawSchedule_DuringHandler_IsRejected()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(10),
            SchedulerPhase.Execution,
            "parent");

        var runner =
            CreateRunner(
                scheduler);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                        scheduler.Schedule(
                            new SimulationTime(20),
                            SchedulerPhase.FollowUp,
                            "illegal")));
    }

    [Fact]
    public void RawDequeue_DuringHandler_IsRejected()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(10),
            SchedulerPhase.Execution,
            "parent");

        scheduler.Schedule(
            new SimulationTime(20),
            SchedulerPhase.Execution,
            "future");

        var runner =
            CreateRunner(
                scheduler);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    context =>
                        scheduler.TryDequeue(
                            out _)));
    }

    [Fact]
    public void RawScheduleFrom_DuringHandler_IsRejected()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(10),
            SchedulerPhase.Execution,
            "parent");

        var runner =
            CreateRunner(
                scheduler);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    context =>
                        scheduler.ScheduleFrom(
                            context.Key,
                            context.CurrentTime,
                            SchedulerPhase.FollowUp,
                            "illegal")));
    }

    private static SimulationRunner<string> CreateRunner(
        SimulationScheduler<string> scheduler)
    {
        return new SimulationRunner<string>(
            scheduler,
            new SimulationRunnerLimits(
                maxProcessedEvents: 100UL));
    }
}