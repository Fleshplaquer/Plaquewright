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
}