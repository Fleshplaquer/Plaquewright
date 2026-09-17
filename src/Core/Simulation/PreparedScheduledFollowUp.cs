namespace Plaquewright.Core.Simulation;

internal sealed class PreparedScheduledFollowUp<TPayload>
    : IDisposable
{
    private SimulationScheduler<TPayload>? _scheduler;

    private readonly ScheduledEventKey _parent;

    private readonly ScheduledEvent<TPayload> _scheduledEvent;

    internal PreparedScheduledFollowUp(
        SimulationScheduler<TPayload> scheduler,
        ScheduledEventKey parent,
        ScheduledEvent<TPayload> scheduledEvent)
    {
        ArgumentNullException.ThrowIfNull(
            scheduler);

        _scheduler =
            scheduler;

        _parent =
            parent;

        _scheduledEvent =
            scheduledEvent;
    }

    internal ScheduledEventKey Key =>
        _scheduledEvent.Key;

    internal ScheduledEventKey Publish()
    {
        var scheduler =
            _scheduler ??
            throw new InvalidOperationException(
                "Prepared follow-up has already been published or cancelled.");

        var key =
            scheduler.PublishPreparedFollowUp(
                _parent,
                _scheduledEvent);

        _scheduler =
            null;

        return key;
    }

    public void Dispose()
    {
        var scheduler =
            _scheduler;

        if (scheduler is null)
        {
            return;
        }

        scheduler.CancelPreparedFollowUp();

        _scheduler =
            null;
    }
}