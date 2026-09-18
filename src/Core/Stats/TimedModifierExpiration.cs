using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Stats;

public readonly record struct
    TimedModifierExpiration
{
    public TimedModifierKey Key { get; }

    public ulong Generation { get; }

    public SimulationTime ExpiresAt { get; }

    public bool IsValid =>
        Key.IsValid &&
        Generation > 0UL;

    internal TimedModifierExpiration(
        TimedModifierKey key,
        ulong generation,
        SimulationTime expiresAt)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException(
                "Timed modifier key must be valid.",
                nameof(key));
        }

        if (generation == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(generation),
                generation,
                "Timed modifier generation must be greater than zero.");
        }

        Key =
            key;

        Generation =
            generation;

        ExpiresAt =
            expiresAt;
    }
}