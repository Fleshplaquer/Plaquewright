using Plaquewright.Core.Composition;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Hosting;

public sealed class SimulationSession<TWorkItem>
{
    private readonly SimulationComposition<TWorkItem>
        _composition;

    private readonly SimulationRunner<TWorkItem>
        _runner;

    public SimulationSession(
        SimulationComposition<TWorkItem> composition,
        SimulationSchedulerLimits schedulerLimits,
        SimulationRunnerLimits runnerLimits)
    {
        ArgumentNullException.ThrowIfNull(
            composition);

        ArgumentNullException.ThrowIfNull(
            schedulerLimits);

        ArgumentNullException.ThrowIfNull(
            runnerLimits);

        _composition =
            composition;

        var scheduler =
            new SimulationScheduler<TWorkItem>(
                schedulerLimits);

        _runner =
            new SimulationRunner<TWorkItem>(
                scheduler,
                runnerLimits);
    }

    internal SimulationRunnerSnapshot<TPayloadSnapshot>
    CaptureRunnerSnapshot<TPayloadSnapshot>(
        Func<TWorkItem, TPayloadSnapshot>
            capturePayload)
    {
        ArgumentNullException.ThrowIfNull(
            capturePayload);

        return _runner.CaptureSnapshot(
            capturePayload);
    }

    internal static SimulationSession<TWorkItem>
        Restore<TPayloadSnapshot>(
            SimulationComposition<TWorkItem> composition,
            SimulationRunnerSnapshot<TPayloadSnapshot>
                runnerSnapshot,
            Func<TPayloadSnapshot, TWorkItem>
                restorePayload)
    {
        ArgumentNullException.ThrowIfNull(
            composition);

        ArgumentNullException.ThrowIfNull(
            runnerSnapshot);

        ArgumentNullException.ThrowIfNull(
            restorePayload);

        var runner =
            SimulationRunner<TWorkItem>.Restore(
                runnerSnapshot,
                restorePayload);

        return new SimulationSession<TWorkItem>(
            composition,
            runner);
    }

    public SimulationTime CurrentTime =>
        _runner.CurrentTime;

    public ulong ProcessedEvents =>
        _runner.ProcessedEvents;

    public ScheduledEventKey ScheduleExternalInput(
        SimulationTime time,
        TWorkItem workItem)
    {
        return _runner.ScheduleExternalInput(
            time,
            workItem);
    }

    public SimulationRunResult RunNext()
    {
        return _runner.RunNext(
            _composition.Execute);
    }

    public SimulationRunResult RunToCompletion()
    {
        return _runner.RunToCompletion(
            _composition.Execute);
    }
    private SimulationSession(
    SimulationComposition<TWorkItem> composition,
    SimulationRunner<TWorkItem> runner)
    {
        ArgumentNullException.ThrowIfNull(
            composition);

        ArgumentNullException.ThrowIfNull(
            runner);

        _composition =
            composition;

        _runner =
            runner;
    }
}