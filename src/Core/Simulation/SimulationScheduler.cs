namespace Idler.Core.Simulation;

public sealed class SimulationScheduler<TPayload>
{
    private readonly PriorityQueue<
        ScheduledEvent<TPayload>,
        ScheduledEventKey> _queue = new();

    private readonly SimulationSchedulerLimits _limits;

    private ulong _nextSequence = 1UL;

    internal SimulationScheduler()
        : this(
            SimulationSchedulerLimits.UnboundedForTests)
    {
    }

    public SimulationScheduler(
        SimulationSchedulerLimits limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        _limits = limits;
    }

    public int Count => _queue.Count;

    public bool IsEmpty => _queue.Count == 0;

    public ScheduledEventKey Schedule(
        SimulationTime time,
        SchedulerPhase phase,
        TPayload payload)
    {
        return Enqueue(
            time,
            SchedulerWave.Initial,
            phase,
            payload);
    }

    public ScheduledEventKey ScheduleFrom(
      ScheduledEventKey parent,
      SimulationTime time,
      SchedulerPhase phase,
      TPayload payload)
    {
        if (time < parent.Time)
        {
            throw new InvalidOperationException(
                "Cannot schedule an event before its parent event.");
        }

        SchedulerWave wave;

        if (time == parent.Time)
        {
            if (parent.Wave.Value >=
                _limits.MaxSameTimestampWave)
            {
                throw new SimulationBudgetExceededException(
                    SimulationBudgetKind.SameTimestampWave,
                    $"Scheduler same-timestamp wave limit of " +
                    $"{_limits.MaxSameTimestampWave} was exceeded.");
            }

            wave = parent.Wave.Next();
        }
        else
        {
            wave = SchedulerWave.Initial;
        }

        return Enqueue(
            time,
            wave,
            phase,
            payload);
    }

    public bool TryPeek(
        out ScheduledEvent<TPayload> scheduledEvent)
    {
        if (_queue.TryPeek(
                out var element,
                out _))
        {
            scheduledEvent = element;
            return true;
        }

        scheduledEvent = default;
        return false;
    }

    public bool TryDequeue(
        out ScheduledEvent<TPayload> scheduledEvent)
    {
        if (_queue.TryDequeue(
                out var element,
                out _))
        {
            scheduledEvent = element;
            return true;
        }

        scheduledEvent = default;
        return false;
    }

    private ScheduledEventKey Enqueue(
        SimulationTime time,
        SchedulerWave wave,
        SchedulerPhase phase,
        TPayload payload)
    {
        ValidateQueueBudget();

        var sequence =
            NextSequence();

        var key =
            new ScheduledEventKey(
                time,
                wave,
                phase,
                sequence);

        var scheduledEvent =
            new ScheduledEvent<TPayload>(
                key,
                payload);

        _queue.Enqueue(
            scheduledEvent,
            key);

        return key;
    }

    private void ValidateQueueBudget()
    {
        if (_queue.Count >=
            _limits.MaxQueueSize)
        {
            throw new SimulationBudgetExceededException(
                SimulationBudgetKind.QueueSize,
                $"Scheduler queue size limit of " +
                $"{_limits.MaxQueueSize} events was exceeded.");
        }
    }

    private ScheduledSequence NextSequence()
    {
        if (_nextSequence == 0UL)
        {
            throw new OverflowException(
                "Scheduler sequence space has been exhausted.");
        }

        var sequence =
            new ScheduledSequence(
                _nextSequence);

        _nextSequence = unchecked(
            _nextSequence + 1UL);

        return sequence;
    }
}