namespace Plaquewright.Core.Stats;

public static class TimedModifierValueQuery
{
    public static double Evaluate(
        double baseValue,
        TimedModifierStateSet modifiers)
    {
        ArgumentNullException.ThrowIfNull(
            modifiers);

        var accumulator =
            new ModifierAccumulator();

        modifiers.AddActiveTo(
            accumulator);

        return accumulator.Apply(
            baseValue);
    }
}