namespace Plaquewright.Core.Simulation;

public sealed class SimulationEventContext<TPayload>
{
    private readonly SimulationScheduler<TPayload> _scheduler;

    internal SimulationEventContext(
        SimulationScheduler<TPayload> scheduler,
        ScheduledEvent<TPayload> currentEvent)
    {
        ArgumentNullException.ThrowIfNull(
            scheduler);

        _scheduler = scheduler;

        Key = currentEvent.Key;
        Payload = currentEvent.Payload;
    }

    public ScheduledEventKey Key { get; }

    public TPayload Payload { get; }

    public SimulationTime CurrentTime =>
        Key.Time;

    public ScheduledEventKey Schedule(
        SimulationTime time,
        SchedulerPhase phase,
        TPayload payload)
    {
        return _scheduler.ScheduleFromActiveEvent(
            Key,
            time,
            phase,
            payload);
    }
    internal PreparedScheduledFollowUp<TPayload>
    PrepareFollowUp(
        TPayload payload)
    {
        return PrepareFollowUp(
            CurrentTime,
            payload);
    }

    internal PreparedScheduledFollowUp<TPayload>
        PrepareFollowUp(
            SimulationTime time,
            TPayload payload)
    {
        return _scheduler.PrepareFollowUpFromActiveEvent(
            Key,
            time,
            payload);
    }
}