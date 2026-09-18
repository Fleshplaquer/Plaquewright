namespace Plaquewright.Core.Simulation;

internal sealed class PreparedScheduledFollowUp<TPayload>
    : IDisposable
{
    private SimulationScheduler<TPayload>? _scheduler;

    private readonly ScheduledEventKey _parent;

    private readonly ScheduledEventKey _key;

    internal PreparedScheduledFollowUp(
        SimulationScheduler<TPayload> scheduler,
        ScheduledEventKey parent,
        ScheduledEventKey key)
    {
        ArgumentNullException.ThrowIfNull(
            scheduler);

        _scheduler =
            scheduler;

        _parent =
            parent;

        _key =
            key;
    }

    internal ScheduledEventKey Key =>
        _key;

    internal ScheduledEventKey Publish(
        TPayload payload)
    {
        var scheduler =
            _scheduler ??
            throw new InvalidOperationException(
                "Prepared follow-up has already been published or cancelled.");

        var key =
            scheduler.PublishPreparedFollowUp(
                _parent,
                _key,
                payload);

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