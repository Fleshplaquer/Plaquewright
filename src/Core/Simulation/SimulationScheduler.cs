namespace Plaquewright.Core.Simulation;

public sealed class SimulationScheduler<TPayload>
{
    private readonly PriorityQueue<
        ScheduledEvent<TPayload>,
        ScheduledEventKey> _queue = new();

    private readonly SimulationSchedulerLimits _limits;

    private ulong _nextSequence = 1UL;
    private int _reservedQueueSlots;

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

    internal ScheduledEventKey Schedule(
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

    internal PreparedScheduledFollowUp<TPayload>
    PrepareFollowUpFromActiveEvent(
        ScheduledEventKey parent,
        SimulationTime time)
    {
        if (_activeEventKey is not { } activeEventKey ||
            activeEventKey != parent)
        {
            throw new InvalidOperationException(
                "The simulation event context is no longer active.");
        }

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

        ValidateQueueBudget();

        var key =
            new ScheduledEventKey(
                time,
                wave,
                SchedulerPhase.FollowUp,
                NextSequence());

        _reservedQueueSlots =
            checked(_reservedQueueSlots + 1);

        return new PreparedScheduledFollowUp<TPayload>(
            this,
            parent,
            key);
    }

    internal SimulationSchedulerSnapshot<TPayloadSnapshot>
    CaptureSnapshot<TPayloadSnapshot>(
        Func<TPayload, TPayloadSnapshot>
            capturePayload)
    {
        ArgumentNullException.ThrowIfNull(
            capturePayload);

        EnsureSnapshotBoundary();

        var pendingEvents =
            _queue.UnorderedItems
                .Select(
                    item => item.Element)
                .OrderBy(
                    scheduledEvent =>
                        scheduledEvent.Key)
                .Select(
                    scheduledEvent =>
                        new ScheduledEventSnapshot<
                            TPayloadSnapshot>(
                            scheduledEvent.Key,
                            capturePayload(
                                scheduledEvent.Payload)))
                .ToArray();

        return new SimulationSchedulerSnapshot<
            TPayloadSnapshot>(
            _limits.MaxQueueSize,
            _limits.MaxSameTimestampWave,
            _nextSequence,
            pendingEvents);
    }

    internal static SimulationScheduler<TPayload>
        Restore<TPayloadSnapshot>(
            SimulationSchedulerSnapshot<TPayloadSnapshot>
                snapshot,
            Func<TPayloadSnapshot, TPayload>
                restorePayload)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        ArgumentNullException.ThrowIfNull(
            restorePayload);

        var scheduler =
            new SimulationScheduler<TPayload>(
                new SimulationSchedulerLimits(
                    snapshot.MaxQueueSize,
                    snapshot.MaxSameTimestampWave));

        scheduler._nextSequence =
            snapshot.NextSequenceValue;

        for (var index = 0;
             index < snapshot.PendingEvents.Count;
             index++)
        {
            var pending =
                snapshot.PendingEvents[index];

            var payload =
                restorePayload(
                    pending.Payload);

            var scheduledEvent =
                new ScheduledEvent<TPayload>(
                    pending.Key,
                    payload);

            //
            // Restore keeps the original authoritative
            // ordering key. Do not call Schedule/Enqueue,
            // because those allocate a new sequence.
            //
            scheduler._queue.Enqueue(
                scheduledEvent,
                pending.Key);
        }

        return scheduler;
    }

    private void EnsureSnapshotBoundary()
    {
        if (_activeEventKey is not null)
        {
            throw new InvalidOperationException(
                "Scheduler snapshot cannot be captured while an event is being executed.");
        }

        if (_reservedQueueSlots != 0)
        {
            throw new InvalidOperationException(
                "Scheduler snapshot cannot be captured while prepared follow-up reservations are outstanding.");
        }
    }

    internal ScheduledEventKey PublishPreparedFollowUp(
    ScheduledEventKey parent,
    ScheduledEventKey key,
    TPayload payload)
    {
        if (_activeEventKey is not { } activeEventKey ||
            activeEventKey != parent)
        {
            throw new InvalidOperationException(
                "Prepared follow-up can only be published while its parent event is active.");
        }

        if (_reservedQueueSlots <= 0)
        {
            throw new InvalidOperationException(
                "No prepared follow-up queue slot is reserved.");
        }

        var scheduledEvent =
            new ScheduledEvent<TPayload>(
                key,
                payload);

        _queue.Enqueue(
            scheduledEvent,
            key);

        _reservedQueueSlots--;

        return key;
    }

    internal void CancelPreparedFollowUp()
    {
        if (_reservedQueueSlots <= 0)
        {
            throw new InvalidOperationException(
                "No prepared follow-up queue slot is reserved.");
        }

        _reservedQueueSlots--;
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
        ValidatePhase(
    phase);

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

    private static void ValidatePhase(
    SchedulerPhase phase)
    {
        if (!Enum.IsDefined(
                phase))
        {
            throw new ArgumentOutOfRangeException(
                nameof(phase),
                phase,
                "Unknown scheduler phase.");
        }
    }

    private void ValidateQueueBudget()
    {
        if ((long)_queue.Count +
                _reservedQueueSlots >=
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