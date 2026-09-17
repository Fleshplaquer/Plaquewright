using System.Reflection;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.ExternalTests.Simulation;

public sealed class SimulationSchedulerInputBoundaryTests
{
    [Fact]
    public void RawPhaseScheduling_IsNotPublic()
    {
        var rawSchedule =
            typeof(SimulationScheduler<string>)
                .GetMethod(
                    "Schedule",
                    BindingFlags.Instance |
                    BindingFlags.Public,
                    binder: null,
                    types:
                    new[]
                    {
                        typeof(SimulationTime),
                        typeof(SchedulerPhase),
                        typeof(string)
                    },
                    modifiers: null);

        Assert.Null(
            rawSchedule);
    }

    [Fact]
    public void ExternalInput_IsAdmittedThroughRunnerWithoutCallerPhaseSelection()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            CreateRunner(
                scheduler);

        var key =
            runner.ScheduleExternalInput(
                new SimulationTime(100L),
                "input");

        Assert.Equal(
    new SimulationTime(100L),
    key.Time);

        Assert.Equal(
            SchedulerWave.Initial,
            key.Wave);

        string? executedPayload = null;

        var result =
            runner.RunNext(
                context =>
                    executedPayload =
                        context.Payload);

        Assert.Equal(
            "input",
            executedPayload);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    [Fact]
    public void ExternalInput_AtStartedTimestamp_IsRejectedWithoutFaultingRunner()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            CreateRunner(
                scheduler);

        //
        // Both inputs are admitted before timestamp 100 starts.
        //
        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            "first");

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            "second");

        string? firstPayload =
            null;

        var firstResult =
            runner.RunNext(
                context =>
                    firstPayload =
                        context.Payload);

        Assert.Equal(
            "first",
            firstPayload);

        Assert.Equal(
            SimulationRunStatus.InProgress,
            firstResult.Status);

        //
        // Timestamp 100 has started. A late host input may
        // no longer join it.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                runner.ScheduleExternalInput(
                    new SimulationTime(100L),
                    "late"));

        //
        // Rejection does not fault the runner.
        //
        runner.ScheduleExternalInput(
            new SimulationTime(101L),
            "future");

        var remainingTrace =
            new List<string>();

        var result =
            runner.RunToCompletion(
                context =>
                    remainingTrace.Add(
                        context.Payload));

        Assert.Equal(
            new[]
            {
            "second",
            "future"
            },
            remainingTrace);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    [Fact]
    public void ExternalInput_BeforeCurrentTime_IsRejectedWithoutFaultingRunner()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            "first");

        runner.RunNext(
            _ =>
            {
            });

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.ScheduleExternalInput(
                    new SimulationTime(99L),
                    "past"));

        runner.ScheduleExternalInput(
            new SimulationTime(101L),
            "future");

        string? executedPayload = null;

        var result =
            runner.RunNext(
                context =>
                    executedPayload =
                        context.Payload);

        Assert.Equal(
            "future",
            executedPayload);

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);
    }

    [Fact]
    public void ExternalInput_AfterTerminalBudget_IsRejected()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            new SimulationRunner<string>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 1UL));

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "first");

        runner.ScheduleExternalInput(
            new SimulationTime(1L),
            "second");

        var result =
            runner.RunToCompletion(
                _ =>
                {
                });

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            result.Status);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.ScheduleExternalInput(
                    new SimulationTime(2L),
                    "late"));
    }

    [Fact]
    public void ExternalInput_AfterRunnerFault_IsRejected()
    {
        var scheduler =
            CreateScheduler();

        var runner =
            CreateRunner(
                scheduler);

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "event");

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.RunNext(
                    _ =>
                        throw new InvalidOperationException(
                            "Gameplay failure.")));

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.ScheduleExternalInput(
                    new SimulationTime(1L),
                    "late"));
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
}