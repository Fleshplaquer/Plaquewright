using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Stats;

public sealed class TimedModifierStateSet
{
    private sealed class Slot
    {
        public ulong Generation;

        public ActiveModifier? Active;
    }

    private sealed record ActiveModifier(
        ModifierKind Kind,
        double Value,
        SimulationTime ExpiresAt);

    private readonly SortedDictionary<
        string,
        Slot> _slots =
            new(
                StringComparer.Ordinal);

    private ulong _revision;

    private int _activeCount;

    public ulong Revision =>
        _revision;

    public int ActiveCount =>
        _activeCount;

    public bool IsActive(
        TimedModifierKey key)
    {
        ValidateKey(
            key);

        return
            _slots.TryGetValue(
                key.Value,
                out var slot) &&
            slot.Active is not null;
    }

    public TimedModifierExpiration
        ApplyOrRefresh(
            TimedModifierKey key,
            ModifierKind kind,
            double value,
            SimulationTime currentTime,
            SimulationDuration duration)
    {
        ValidateKey(
            key);

        if (duration ==
            SimulationDuration.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Timed modifier duration must be greater than zero.");
        }

        //
        // Perform every failure-capable calculation before
        // mutating authoritative state.
        //
        var expiresAt =
            currentTime +
            duration;

        ValidateContribution(
            kind,
            value);

        EnsureRevisionCanAdvance();

        _slots.TryGetValue(
            key.Value,
            out var existingSlot);

        if (existingSlot is not null &&
            existingSlot.Generation ==
            ulong.MaxValue)
        {
            throw new InvalidOperationException(
                "Timed modifier generation is exhausted.");
        }

        var generation =
            existingSlot is null
                ? 1UL
                : existingSlot.Generation + 1UL;

        var slot =
            existingSlot ??
            new Slot();

        var wasActive =
            slot.Active is not null;

        slot.Generation =
            generation;

        slot.Active =
            new ActiveModifier(
                kind,
                value,
                expiresAt);

        if (existingSlot is null)
        {
            _slots.Add(
                key.Value,
                slot);
        }

        if (!wasActive)
        {
            _activeCount++;
        }

        AdvanceRevision();

        return new TimedModifierExpiration(
            key,
            generation,
            expiresAt);
    }

    public bool Cancel(
        TimedModifierKey key)
    {
        ValidateKey(
            key);

        if (!_slots.TryGetValue(
                key.Value,
                out var slot) ||
            slot.Active is null)
        {
            return false;
        }

        EnsureRevisionCanAdvance();

        slot.Active =
            null;

        _activeCount--;

        AdvanceRevision();

        return true;
    }

    public bool Expire(
        TimedModifierExpiration expiration,
        SimulationTime currentTime)
    {
        if (!expiration.IsValid)
        {
            throw new ArgumentException(
                "Timed modifier expiration must be valid.",
                nameof(expiration));
        }

        if (!_slots.TryGetValue(
                expiration.Key.Value,
                out var slot) ||
            slot.Active is null ||
            slot.Generation !=
                expiration.Generation)
        {
            //
            // Expected stale-event path after refresh/cancel.
            //
            return false;
        }

        if (slot.Active.ExpiresAt !=
            expiration.ExpiresAt)
        {
            throw new InvalidOperationException(
                "Timed modifier expiration no longer matches the active state.");
        }

        if (currentTime !=
            expiration.ExpiresAt)
        {
            throw new InvalidOperationException(
                "Timed modifier can only expire at its declared expiration time.");
        }

        EnsureRevisionCanAdvance();

        slot.Active =
            null;

        _activeCount--;

        AdvanceRevision();

        return true;
    }

    internal TimedModifierStateSetSnapshot
    CaptureSnapshot()
    {
        var snapshots =
            new TimedModifierSlotSnapshot[
                _slots.Count];

        var index =
            0;

        foreach (var pair in
                 _slots)
        {
            TimedModifierActiveSnapshot?
                activeSnapshot =
                    null;

            if (pair.Value.Active is { } active)
            {
                activeSnapshot =
                    new TimedModifierActiveSnapshot(
                        active.Kind,
                        active.Value,
                        active.ExpiresAt);
            }

            snapshots[index] =
                new TimedModifierSlotSnapshot(
                    TimedModifierKey.Parse(
                        pair.Key),
                    pair.Value.Generation,
                    activeSnapshot);

            index++;
        }

        return new TimedModifierStateSetSnapshot(
            _revision,
            snapshots);
    }

    internal static TimedModifierStateSet Restore(
        TimedModifierStateSetSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        var state =
            new TimedModifierStateSet();

        foreach (var slotSnapshot in
                 snapshot.Slots)
        {
            var slot =
                new Slot
                {
                    Generation =
                        slotSnapshot.Generation
                };

            if (slotSnapshot.Active is { } active)
            {
                //
                // Validate again at the restore boundary.
                //
                ValidateContribution(
                    active.Kind,
                    active.Value);

                slot.Active =
                    new ActiveModifier(
                        active.Kind,
                        active.Value,
                        active.ExpiresAt);

                state._activeCount++;
            }

            state._slots.Add(
                slotSnapshot.Key.Value,
                slot);
        }

        state._revision =
            snapshot.Revision;

        return state;
    }

    internal void AddActiveTo(
        ModifierAccumulator accumulator)
    {
        ArgumentNullException.ThrowIfNull(
            accumulator);

        foreach (var slot in
                 _slots.Values)
        {
            if (slot.Active is not { } active)
            {
                continue;
            }

            accumulator.Add(
                active.Kind,
                active.Value);
        }
    }

    private static void ValidateContribution(
        ModifierKind kind,
        double value)
    {
        //
        // Reuse ModifierAccumulator as the semantic
        // validation authority.
        //
        var accumulator =
            new ModifierAccumulator();

        accumulator.Add(
            kind,
            value);
    }

    private static void ValidateKey(
        TimedModifierKey key)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException(
                "Timed modifier key must be valid.",
                nameof(key));
        }
    }

    private void EnsureRevisionCanAdvance()
    {
        if (_revision ==
            ulong.MaxValue)
        {
            throw new InvalidOperationException(
                "Timed modifier state revision is exhausted.");
        }
    }

    private void AdvanceRevision()
    {
        _revision++;
    }
}