using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRunnerTests
{
    [Fact]
    public void EmptyScheduler_IsImmediatelyCompleted()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            CreateRunner(
                scheduler,
                maxProcessedEvents: 100UL);

        var executed = false;

        var result =
            runner.RunNext(
                _ => executed = true);

        Assert.False(executed);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.True(
            result.ResultComplete);

        Assert.Equal(
            0UL,
            result.ProcessedEvents);

        Assert.Equal(
            0,
            result.PendingEvents);

        Assert.Null(
            result.BudgetKind);
    }

    [Fact]
    public void RunNext_ExecutesEarliestEvent()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            new SimulationTime(200L),
            SchedulerPhase.Execution,
            "second");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "first");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        string? executed = null;

        var result =
            runner.RunNext(
                scheduledEvent =>
                    executed =
                        scheduledEvent.Payload);

        Assert.Equal(
            "first",
            executed);

        Assert.Equal(
            new SimulationTime(100L),
            runner.CurrentTime);

        Assert.Equal(
            1UL,
            runner.ProcessedEvents);

        Assert.Equal(
            SimulationRunStatus.InProgress,
            result.Status);

        Assert.False(
            result.ResultComplete);
    }

    [Fact]
    public void RunToCompletion_ExecutesAllEventsInSchedulerOrder()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            new SimulationTime(300L),
            SchedulerPhase.Execution,
            "third");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            new SimulationTime(200L),
            SchedulerPhase.Execution,
            "second");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        var executed =
            new List<string>();

        var result =
            runner.RunToCompletion(
                scheduledEvent =>
                    executed.Add(
                        scheduledEvent.Payload));

        Assert.Equal(
            ["first", "second", "third"],
            executed);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.True(
            result.ResultComplete);

        Assert.Equal(
            3UL,
            result.ProcessedEvents);

        Assert.Equal(
            new SimulationTime(300L),
            result.CurrentTime);

        Assert.Equal(
            0,
            result.PendingEvents);
    }

    [Fact]
    public void ProcessedEventBudget_AllowsExactlyConfiguredAmountWhenQueueEmpties()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        var runner =
            CreateRunner(
                scheduler,
                maxProcessedEvents: 2UL);

        var result =
            runner.RunToCompletion(
                _ =>
                {
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.True(
            result.ResultComplete);

        Assert.Equal(
            2UL,
            result.ProcessedEvents);
    }

    [Fact]
    public void ProcessedEventBudget_AbortsWhenWorkRemains()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "third");

        var runner =
            CreateRunner(
                scheduler,
                maxProcessedEvents: 2UL);

        var executed =
            new List<string>();

        var result =
            runner.RunToCompletion(
                scheduledEvent =>
                    executed.Add(
                        scheduledEvent.Payload));

        Assert.Equal(
            ["first", "second"],
            executed);

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            result.Status);

        Assert.False(
            result.ResultComplete);

        Assert.Equal(
            SimulationBudgetKind.ProcessedEvents,
            result.BudgetKind);

        Assert.Equal(
            2UL,
            result.ProcessedEvents);

        Assert.Equal(
            1,
            result.PendingEvents);
    }

    [Fact]
    public void SchedulerBudgetException_IsConvertedIntoIncompleteResult()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 1,
                    maxSameTimestampWave: 10));

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "parent");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        var result =
            runner.RunNext(
                context =>
    {
        context.Schedule(
            context.CurrentTime,
            SchedulerPhase.FollowUp,
                        "first-child");

        context.Schedule(
            context.CurrentTime,
SchedulerPhase.Execution,
            "second-child");
    });

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            result.Status);

        Assert.False(
            result.ResultComplete);

        Assert.Equal(
            SimulationBudgetKind.QueueSize,
            result.BudgetKind);

        Assert.Equal(
            0UL,
            result.ProcessedEvents);
    }

    [Fact]
    public void BudgetExceeded_IsTerminalForRunner()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        var runner =
            CreateRunner(
                scheduler,
                maxProcessedEvents: 1UL);

        var executionCount = 0;

        var firstResult =
            runner.RunToCompletion(
                _ => executionCount++);

        var secondResult =
            runner.RunNext(
                _ => executionCount++);

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            firstResult.Status);

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            secondResult.Status);

        Assert.Equal(
            1,
            executionCount);
    }

    [Fact]
    public void NonBudgetException_IsNotSwallowed()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "event");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                        throw new InvalidOperationException(
                            "Gameplay bug")));
    }

    [Fact]
    public void Runner_RejectsTimeMovingBackward()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "first");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        runner.RunNext(
            _ =>
            {
            });

        scheduler.Schedule(
            new SimulationTime(99L),
            SchedulerPhase.Execution,
            "invalid-past-event");

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                    {
                    }));
    }

    [Fact]
    public void ZeroDelayChild_IsExecutedAfterRemainingCurrentWave()
    {
        var scheduler =
            CreateScheduler();

        var parent =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "parent");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.FollowUp,
            "remaining-wave-zero");

        var runner =
            CreateRunner(
                scheduler,
                100UL);

        var executed =
            new List<string>();

        var result =
            runner.RunToCompletion(
                scheduledEvent =>
                {
                    executed.Add(
                        scheduledEvent.Payload);

                    if (scheduledEvent.Key ==
                        parent)
                    {
                        scheduledEvent.Schedule(
                            scheduledEvent.CurrentTime,
                            SchedulerPhase.FollowUp,
                            "child-wave-one");
                    }
                });

        Assert.Equal(
            [
                "parent",
                "remaining-wave-zero",
                "child-wave-one"
            ],
            executed);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    private static SimulationScheduler<string>
        CreateScheduler()
    {
        return new SimulationScheduler<string>(
            new SimulationSchedulerLimits(
                maxQueueSize: 1000,
                maxSameTimestampWave: 100));
    }

    private static SimulationRunner<string>
        CreateRunner(
            SimulationScheduler<string> scheduler,
            ulong maxProcessedEvents)
    {
        return new SimulationRunner<string>(
            scheduler,
            new SimulationRunnerLimits(
                maxProcessedEvents));
    }
}