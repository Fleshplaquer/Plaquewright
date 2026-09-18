using System.Collections.ObjectModel;

namespace Plaquewright.Core.Simulation;

internal sealed class SimulationSchedulerSnapshot<TPayloadSnapshot>
{
    private readonly ReadOnlyCollection<
        ScheduledEventSnapshot<TPayloadSnapshot>>
        _pendingEvents;

    public int MaxQueueSize { get; }

    public uint MaxSameTimestampWave { get; }

    //
    // Zero is meaningful here:
    // it represents exhausted scheduler sequence space.
    //
    public ulong NextSequenceValue { get; }

    public IReadOnlyList<
        ScheduledEventSnapshot<TPayloadSnapshot>>
        PendingEvents =>
            _pendingEvents;

    internal SimulationSchedulerSnapshot(
        int maxQueueSize,
        uint maxSameTimestampWave,
        ulong nextSequenceValue,
        IReadOnlyList<
            ScheduledEventSnapshot<TPayloadSnapshot>>
            pendingEvents)
    {
        if (maxQueueSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxQueueSize));
        }

        ArgumentNullException.ThrowIfNull(
            pendingEvents);

        if (pendingEvents.Count >
            maxQueueSize)
        {
            throw new ArgumentException(
                "Scheduler snapshot contains more pending events than its queue limit allows.",
                nameof(pendingEvents));
        }

        MaxQueueSize =
            maxQueueSize;

        MaxSameTimestampWave =
            maxSameTimestampWave;

        NextSequenceValue =
            nextSequenceValue;

        _pendingEvents =
            Array.AsReadOnly(
                pendingEvents.ToArray());
    }
}