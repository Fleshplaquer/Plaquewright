using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class PreparedScheduledFollowUpTests
{
    [Fact]
    public void PreparedFollowUp_ReservesQueueCapacityWithoutPublishing()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 1);

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "parent");

        var result =
            runner.RunNext(
                context =>
                {
                    using var prepared =
                        context.PrepareFollowUp(
                            "reserved");

                    Assert.Throws<SimulationBudgetExceededException>(
                        () =>
                            context.Schedule(
                                context.CurrentTime,
                                SchedulerPhase.FollowUp,
                                "other"));
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    [Fact]
    public void Publish_UsesReservedCapacity()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 1);

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "parent");

        var first =
            runner.RunNext(
                context =>
                {
                    using var prepared =
                        context.PrepareFollowUp(
                            "child");

                    prepared.Publish();
                });

        Assert.Equal(
            SimulationRunStatus.InProgress,
            first.Status);

        string? executedPayload =
            null;

        var second =
            runner.RunNext(
                context =>
                    executedPayload =
                        context.Payload);

        Assert.Equal(
            "child",
            executedPayload);

        Assert.Equal(
            SimulationRunStatus.Completed,
            second.Status);
    }

    [Fact]
    public void Dispose_ReleasesReservedCapacity()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 1);

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "parent");

        runner.RunNext(
            context =>
            {
                using (context.PrepareFollowUp(
                           "cancelled"))
                {
                }

                context.Schedule(
                    context.CurrentTime,
                    SchedulerPhase.FollowUp,
                    "replacement");
            });

        string? executedPayload =
            null;

        var result =
            runner.RunNext(
                context =>
                    executedPayload =
                        context.Payload);

        Assert.Equal(
            "replacement",
            executedPayload);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    [Fact]
    public void PreparedFollowUp_CannotBePublishedTwice()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 1);

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "parent");

        runner.RunNext(
            context =>
            {
                using var prepared =
                    context.PrepareFollowUp(
                        "child");

                prepared.Publish();

                Assert.Throws<InvalidOperationException>(
                    () =>
                        prepared.Publish());
            });

        var result =
            runner.RunNext(
                _ =>
                {
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    private static SimulationScheduler<string>
        CreateScheduler(
            int maxQueueSize)
    {
        return new SimulationScheduler<string>(
            new SimulationSchedulerLimits(
                maxQueueSize,
                maxSameTimestampWave: 10));
    }

    private static SimulationRunner<string>
        CreateRunner(
            SimulationScheduler<string> scheduler)
    {
        return new SimulationRunner<string>(
            scheduler,
            new SimulationRunnerLimits(
                maxProcessedEvents: 10UL));
    }
}