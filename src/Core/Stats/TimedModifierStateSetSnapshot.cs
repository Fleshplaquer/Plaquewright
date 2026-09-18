namespace Plaquewright.Core.Stats;

internal sealed class TimedModifierStateSetSnapshot
{
    private readonly TimedModifierSlotSnapshot[]
        _slots;

    public ulong Revision { get; }

    public IReadOnlyList<TimedModifierSlotSnapshot>
        Slots
    { get; }

    internal TimedModifierStateSetSnapshot(
        ulong revision,
        IReadOnlyList<TimedModifierSlotSnapshot> slots)
    {
        ArgumentNullException.ThrowIfNull(
            slots);

        _slots =
            new TimedModifierSlotSnapshot[
                slots.Count];

        var keys =
            new HashSet<string>(
                StringComparer.Ordinal);

        for (var index = 0;
             index < slots.Count;
             index++)
        {
            var slot =
                slots[index];

            if (slot is null)
            {
                throw new ArgumentException(
                    "Timed modifier snapshot cannot contain null slots.",
                    nameof(slots));
            }

            if (!keys.Add(
                    slot.Key.Value))
            {
                throw new ArgumentException(
                    $"Timed modifier snapshot contains duplicate key '{slot.Key}'.",
                    nameof(slots));
            }

            _slots[index] =
                slot;
        }

        Revision =
            revision;

        Slots =
            Array.AsReadOnly(
                _slots);
    }
}