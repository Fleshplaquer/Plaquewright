namespace Plaquewright.Core.Stats;

internal sealed class TimedModifierValueQueryCache
{
    private TimedModifierStateSet? _state;

    private ulong _revision;

    private double _baseValue;

    private double _value;

    private bool _hasValue;

    internal bool TryGetCached(
        double baseValue,
        TimedModifierStateSet modifiers,
        out double value)
    {
        ArgumentNullException.ThrowIfNull(
            modifiers);

        ValidateBaseValue(
            baseValue);

        if (_hasValue &&
            ReferenceEquals(
                _state,
                modifiers) &&
            _revision ==
                modifiers.Revision &&
            _baseValue ==
                baseValue)
        {
            value =
                _value;

            return true;
        }

        value =
            default;

        return false;
    }

    internal double Evaluate(
        double baseValue,
        TimedModifierStateSet modifiers)
    {
        ArgumentNullException.ThrowIfNull(
            modifiers);

        if (TryGetCached(
                baseValue,
                modifiers,
                out var cached))
        {
            return cached;
        }

        var value =
            TimedModifierValueQuery.Evaluate(
                baseValue,
                modifiers);

        _state =
            modifiers;

        _revision =
            modifiers.Revision;

        _baseValue =
            baseValue;

        _value =
            value;

        _hasValue =
            true;

        return value;
    }

    private static void ValidateBaseValue(
        double baseValue)
    {
        if (!double.IsFinite(
                baseValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseValue),
                baseValue,
                "Base value must be finite.");
        }
    }
}