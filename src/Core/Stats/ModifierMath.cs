namespace Idler.Core.Stats;

public static class ModifierMath
{
    public static double Apply(
        double baseValue,
        double flat = 0d,
        double increased = 0d,
        double reduced = 0d,
        IEnumerable<double>? more = null,
        IEnumerable<double>? less = null)
    {
        ValidateFinite(baseValue, nameof(baseValue));
        ValidateFinite(flat, nameof(flat));

        ValidateNonNegativeFinite(
            increased,
            nameof(increased));

        ValidateNonNegativeFinite(
            reduced,
            nameof(reduced));

        var result =
            baseValue + flat;

        ValidateResult(result);

        result *=
            1d + increased - reduced;

        ValidateResult(result);

        ApplyMore(
            ref result,
            more);

        ApplyLess(
            ref result,
            less);

        return result;
    }

    private static void ApplyMore(
        ref double result,
        IEnumerable<double>? modifiers)
    {
        if (modifiers is null)
        {
            return;
        }

        // Multiplication is mathematically commutative,
        // but floating-point rounding can depend on order.
        // Sorting gives the reference implementation
        // deterministic behaviour independent of input order.
        var ordered = modifiers
            .Select(ValidateMore)
            .OrderBy(value => value)
            .ToArray();

        foreach (var modifier in ordered)
        {
            result *=
                1d + modifier;

            ValidateResult(result);
        }
    }

    private static void ApplyLess(
        ref double result,
        IEnumerable<double>? modifiers)
    {
        if (modifiers is null)
        {
            return;
        }

        var ordered = modifiers
            .Select(ValidateLess)
            .OrderBy(value => value)
            .ToArray();

        foreach (var modifier in ordered)
        {
            result *=
                1d - modifier;

            ValidateResult(result);
        }
    }

    private static double ValidateMore(
        double value)
    {
        ValidateNonNegativeFinite(
            value,
            nameof(value));

        return value;
    }

    private static double ValidateLess(
        double value)
    {
        ValidateNonNegativeFinite(
            value,
            nameof(value));

        if (value > 1d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Less modifiers cannot exceed 100%.");
        }

        return value;
    }

    private static void ValidateNonNegativeFinite(
        double value,
        string parameterName)
    {
        ValidateFinite(
            value,
            parameterName);

        if (value < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Modifier magnitude cannot be negative.");
        }
    }

    private static void ValidateFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Modifier values must be finite.");
        }
    }

    private static void ValidateResult(
        double value)
    {
        if (!double.IsFinite(value))
        {
            throw new OverflowException(
                "Modifier calculation produced a non-finite result.");
        }
    }
}