namespace Idler.Core.Simulation;

public sealed class SimulationScheduler<TPayload>
{
    private readonly PriorityQueue<
        ScheduledEvent<TPayload>,
        ScheduledEventKey> _queue = new();

    private readonly SimulationSchedulerLimits _limits;

    private ulong _nextSequence = 1UL;

    private ScheduledEventKey? _activeEventKey;

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

    public int Count
    {
        get
        {
            EnsureOutsideEventExecution();

            return _queue.Count;
        }
    }

    public bool IsEmpty
    {
        get
        {
            EnsureOutsideEventExecution();

            return _queue.Count == 0;
        }
    }

    public ScheduledEventKey Schedule(
        SimulationTime time,
        SchedulerPhase phase,
        TPayload payload)
    {
        EnsureOutsideEventExecution();

        return Enqueue(
            time,
            SchedulerWave.Initial,
            phase,
            payload);
    }

    internal ScheduledEventKey ScheduleFrom(
        ScheduledEventKey parent,
        SimulationTime time,
        SchedulerPhase phase,
        TPayload payload)
    {
        EnsureOutsideEventExecution();

        return ScheduleFromCore(
            parent,
            time,
            phase,
            payload);
    }

    internal ScheduledEventKey ScheduleFromActiveEvent(
        ScheduledEventKey parent,
        SimulationTime time,
        SchedulerPhase phase,
        TPayload payload)
    {
        if (_activeEventKey is not { } activeEventKey ||
            activeEventKey != parent)
        {
            throw new InvalidOperationException(
                "The simulation event context is no longer active.");
        }

        return ScheduleFromCore(
            parent,
            time,
            phase,
            payload);
    }

    public bool TryPeek(
        out ScheduledEvent<TPayload> scheduledEvent)
    {
        EnsureOutsideEventExecution();

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
        EnsureOutsideEventExecution();

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

    internal void BeginEventExecution(
        ScheduledEventKey eventKey)
    {
        if (_activeEventKey is not null)
        {
            throw new InvalidOperationException(
                "A scheduler event is already being executed.");
        }

        _activeEventKey = eventKey;
    }

    internal void EndEventExecution(
        ScheduledEventKey eventKey)
    {
        if (_activeEventKey is not { } activeEventKey ||
            activeEventKey != eventKey)
        {
            throw new InvalidOperationException(
                "Scheduler event execution state is inconsistent.");
        }

        _activeEventKey = null;
    }

    private ScheduledEventKey ScheduleFromCore(
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

            wave =
                parent.Wave.Next();
        }
        else
        {
            wave =
                SchedulerWave.Initial;
        }

        return Enqueue(
            time,
            wave,
            phase,
            payload);
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

    private void EnsureOutsideEventExecution()
    {
        if (_activeEventKey is not null)
        {
            throw new InvalidOperationException(
                "Raw scheduler access is not allowed while a scheduled event " +
                "is being executed. Use the simulation event context.");
        }
    }
}