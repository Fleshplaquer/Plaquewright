namespace Idler.Core.Simulation;

public sealed class SimulationRunnerLimits
{
    public ulong MaxProcessedEvents { get; }

    public SimulationRunnerLimits(
        ulong maxProcessedEvents)
    {
        if (maxProcessedEvents == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxProcessedEvents),
                maxProcessedEvents,
                "Maximum processed event count must be greater than zero.");
        }

        MaxProcessedEvents =
            maxProcessedEvents;
    }
}