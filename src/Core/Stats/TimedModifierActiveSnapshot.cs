using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Stats;

internal sealed class TimedModifierActiveSnapshot
{
    public ModifierKind Kind { get; }

    public double Value { get; }

    public SimulationTime ExpiresAt { get; }

    internal TimedModifierActiveSnapshot(
        ModifierKind kind,
        double value,
        SimulationTime expiresAt)
    {
        //
        // Reuse the existing modifier semantics as the
        // validation authority.
        //
        var validation =
            new ModifierAccumulator();

        validation.Add(
            kind,
            value);

        Kind =
            kind;

        Value =
            value;

        ExpiresAt =
            expiresAt;
    }
}