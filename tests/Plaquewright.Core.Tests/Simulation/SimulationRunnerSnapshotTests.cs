using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRunnerSnapshotTests
{
    [Fact]
    public void SnapshotRestore_PreservesRunnerStateInputClosureAndContinuation()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 10,
                    maxSameTimestampWave: 10));

        var original =
            new SimulationRunner<string>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 100UL));

        original.ScheduleExternalInput(
            new SimulationTime(10L),
            "first");

        original.ScheduleExternalInput(
            new SimulationTime(20L),
            "second");

        var firstResult =
            original.RunNext(
                _ =>
                {
                });

        Assert.Equal(
            SimulationRunStatus.InProgress,
            firstResult.Status);

        Assert.Equal(
            new SimulationTime(10L),
            original.CurrentTime);

        Assert.Equal(
            1UL,
            original.ProcessedEvents);

        var snapshot =
            original.CaptureSnapshot(
                static payload => payload);

        var restored =
            SimulationRunner<string>.Restore(
                snapshot,
                static payload => payload);

        Assert.Equal(
            original.CurrentTime,
            restored.CurrentTime);

        Assert.Equal(
            original.ProcessedEvents,
            restored.ProcessedEvents);

        //
        // D-04 must survive restore.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                original.ScheduleExternalInput(
                    new SimulationTime(10L),
                    "too-late"));

        Assert.Throws<InvalidOperationException>(
            () =>
                restored.ScheduleExternalInput(
                    new SimulationTime(10L),
                    "too-late"));

        var originalMiddleKey =
            original.ScheduleExternalInput(
                new SimulationTime(15L),
                "middle");

        var restoredMiddleKey =
            restored.ScheduleExternalInput(
                new SimulationTime(15L),
                "middle");

        Assert.Equal(
            originalMiddleKey,
            restoredMiddleKey);

        var originalResult =
            original.RunToCompletion(
                _ =>
                {
                });

        var restoredResult =
            restored.RunToCompletion(
                _ =>
                {
                });

        Assert.Equal(
            originalResult,
            restoredResult);

        Assert.Equal(
            new SimulationTime(20L),
            restored.CurrentTime);

        Assert.Equal(
            3UL,
            restored.ProcessedEvents);
    }

    [Fact]
    public void SnapshotCapture_AfterRunnerFault_IsRejected()
    {
        var runner =
            new SimulationRunner<string>(
                new SimulationScheduler<string>(),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "boom");

        Assert.Throws<ApplicationException>(
            () =>
                runner.RunNext(
                    _ =>
                        throw new ApplicationException(
                            "Expected test fault.")));

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.CaptureSnapshot(
                    static payload => payload));
    }

    [Fact]
    public void SnapshotCapture_AfterTerminalBudget_IsRejected()
    {
        var runner =
            new SimulationRunner<string>(
                new SimulationScheduler<string>(),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 1UL));

        runner.ScheduleExternalInput(
            SimulationTime.Zero,
            "first");

        runner.ScheduleExternalInput(
            new SimulationTime(1L),
            "second");

        var result =
            runner.RunNext(
                _ =>
                {
                });

        Assert.Equal(
            SimulationRunStatus.BudgetExceeded,
            result.Status);

        Assert.Equal(
            SimulationBudgetKind.ProcessedEvents,
            result.BudgetKind);

        Assert.Throws<InvalidOperationException>(
            () =>
                runner.CaptureSnapshot(
                    static payload => payload));
    }
}