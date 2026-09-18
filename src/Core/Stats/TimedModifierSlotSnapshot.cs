namespace Plaquewright.Core.Stats;

internal sealed class TimedModifierSlotSnapshot
{
    public TimedModifierKey Key { get; }

    public ulong Generation { get; }

    public TimedModifierActiveSnapshot? Active { get; }

    internal TimedModifierSlotSnapshot(
        TimedModifierKey key,
        ulong generation,
        TimedModifierActiveSnapshot? active)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException(
                "Timed modifier snapshot key must be valid.",
                nameof(key));
        }

        if (generation == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(generation),
                generation,
                "Timed modifier snapshot generation must be greater than zero.");
        }

        Key =
            key;

        Generation =
            generation;

        Active =
            active;
    }
}