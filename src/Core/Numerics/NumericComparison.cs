namespace Plaquewright.Core.Numerics;

internal static class NumericComparison
{
    //
    // Machine epsilon around 1.0 for IEEE-754 double.
    //
    // This is intentionally NOT double.Epsilon.
    //
    private static readonly double RelativeTolerance =
        16d *
        (Math.BitIncrement(1d) - 1d);

    public static bool AreEquivalent(
        double left,
        double right,
        double scale)
    {
        ValidateFinite(
            left,
            nameof(left));

        ValidateFinite(
            right,
            nameof(right));

        ValidateScale(
            scale);

        if (left == right)
        {
            return true;
        }

        var difference =
            Math.Abs(
                left - right);

        var referenceScale =
            Math.Max(
                scale,
                Math.Max(
                    Math.Abs(left),
                    Math.Abs(right)));

        if (referenceScale == 0d)
        {
            return false;
        }

        var tolerance =
            referenceScale *
            RelativeTolerance;

        return difference <=
            tolerance;
    }

    private static void ValidateFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(
                value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Numeric comparison values must be finite.");
        }
    }

    private static void ValidateScale(
        double scale)
    {
        if (!double.IsFinite(
                scale) ||
            scale < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                "Numeric comparison scale must be finite and non-negative.");
        }
    }
}