namespace Plaquewright.Core.Simulation;

public readonly record struct ScheduledEvent<TPayload>
{
    public ScheduledEventKey Key { get; }

    public TPayload Payload { get; }

    public ScheduledEvent(
        ScheduledEventKey key,
        TPayload payload)
    {
        Key = key;
        Payload = payload;
    }
}