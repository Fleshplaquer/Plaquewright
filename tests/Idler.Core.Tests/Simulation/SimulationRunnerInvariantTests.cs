using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationRunnerInvariantTests
{
    [Fact]
    public void RepeatedRunNext_AndRunToCompletion_ProduceSameTrace()
    {
        var firstScheduler =
            CreateScheduler();

        var secondScheduler =
            CreateScheduler();

        Populate(firstScheduler);
        Populate(secondScheduler);

        var firstRunner =
            CreateRunner(firstScheduler);

        var secondRunner =
            CreateRunner(secondScheduler);

        var firstTrace =
            new List<string>();

        SimulationRunResult firstResult;

        while (true)
        {
            firstResult =
                firstRunner.RunNext(
                    scheduledEvent =>
                        firstTrace.Add(
                            scheduledEvent.Payload));

            if (firstResult.Status !=
                SimulationRunStatus.InProgress)
            {
                break;
            }
        }

        var secondTrace =
            new List<string>();

        var secondResult =
            secondRunner.RunToCompletion(
                scheduledEvent =>
                    secondTrace.Add(
                        scheduledEvent.Payload));

        Assert.Equal(
            firstTrace,
            secondTrace);

        Assert.Equal(
            firstResult,
            secondResult);
    }

    [Fact]
    public void SuccessfulRun_NeverMovesCurrentTimeBackward()
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
            CreateRunner(scheduler);

        var observedTimes =
            new List<SimulationTime>();

        var result =
            runner.RunToCompletion(
                _ =>
                    observedTimes.Add(
                        runner.CurrentTime));

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        for (var index = 1;
             index < observedTimes.Count;
             index++)
        {
            Assert.True(
                observedTimes[index] >=
                observedTimes[index - 1]);
        }
    }

    [Fact]
    public void UnexpectedHandlerException_MakesRunnerUnusable()
    {
        var scheduler =
            CreateScheduler();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            new SimulationTime(1L),
            SchedulerPhase.Execution,
            "second");

        var runner =
            CreateRunner(scheduler);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                        throw new InvalidOperationException(
                            "Unexpected failure.")));

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    runner.RunNext(
                        _ =>
                        {
                        }));

        Assert.Contains(
            "faulted",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0UL,
            runner.ProcessedEvents);
    }

    private static SimulationScheduler<string>
        CreateScheduler()
    {
        return new SimulationScheduler<string>(
            new SimulationSchedulerLimits(
                maxQueueSize: 100,
                maxSameTimestampWave: 10));
    }

    private static SimulationRunner<string>
        CreateRunner(
            SimulationScheduler<string> scheduler)
    {
        return new SimulationRunner<string>(
            scheduler,
            new SimulationRunnerLimits(
                maxProcessedEvents: 100UL));
    }

    private static void Populate(
        SimulationScheduler<string> scheduler)
    {
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
    }
}