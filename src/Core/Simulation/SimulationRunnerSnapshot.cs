namespace Plaquewright.Core.Simulation;

internal sealed class SimulationRunnerSnapshot<TPayloadSnapshot>
{
    public ulong MaxProcessedEvents { get; }

    public SimulationTime CurrentTime { get; }

    public ulong ProcessedEvents { get; }

    public SimulationTime? ExternalInputsClosedThrough { get; }

    public SimulationSchedulerSnapshot<TPayloadSnapshot>
        Scheduler
    { get; }

    internal SimulationRunnerSnapshot(
        ulong maxProcessedEvents,
        SimulationTime currentTime,
        ulong processedEvents,
        SimulationTime? externalInputsClosedThrough,
        SimulationSchedulerSnapshot<TPayloadSnapshot>
            scheduler)
    {
        if (maxProcessedEvents == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxProcessedEvents));
        }

        ArgumentNullException.ThrowIfNull(
            scheduler);

        if (externalInputsClosedThrough is { } closedThrough &&
            closedThrough > currentTime)
        {
            throw new ArgumentException(
                "External input closure cannot be later than the current simulation time.",
                nameof(externalInputsClosedThrough));
        }

        MaxProcessedEvents =
            maxProcessedEvents;

        CurrentTime =
            currentTime;

        ProcessedEvents =
            processedEvents;

        ExternalInputsClosedThrough =
            externalInputsClosedThrough;

        Scheduler =
            scheduler;
    }
}