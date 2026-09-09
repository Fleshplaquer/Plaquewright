namespace Idler.Core.Simulation;

public sealed class SimulationSchedulerLimits
{
    public int MaxQueueSize { get; }

    public uint MaxSameTimestampWave { get; }

    public SimulationSchedulerLimits(
        int maxQueueSize,
        uint maxSameTimestampWave)
    {
        if (maxQueueSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxQueueSize),
                maxQueueSize,
                "Maximum queue size must be greater than zero.");
        }

        MaxQueueSize = maxQueueSize;
        MaxSameTimestampWave = maxSameTimestampWave;
    }

    internal static SimulationSchedulerLimits UnboundedForTests { get; } =
        new(
            int.MaxValue,
            uint.MaxValue);
}