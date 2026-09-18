namespace Plaquewright.Core.Simulation;

internal readonly record struct ScheduledEventSnapshot<TPayloadSnapshot>
{
    public ScheduledEventKey Key { get; }

    public TPayloadSnapshot Payload { get; }

    internal ScheduledEventSnapshot(
        ScheduledEventKey key,
        TPayloadSnapshot payload)
    {
        Key =
            key;

        Payload =
            payload;
    }
}