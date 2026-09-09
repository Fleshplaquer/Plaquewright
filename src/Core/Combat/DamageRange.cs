namespace Idler.Core.Combat;

public readonly record struct DamageRange
{
    public double Min { get; }

    public double Max { get; }

    public double Average => (Min / 2d) + (Max / 2d);

    public bool IsCollapsed => Min == Max;

    public DamageRange(double min, double max)
    {
        ValidateFinite(min, nameof(min));
        ValidateFinite(max, nameof(max));

        if (min < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(min),
                min,
                "DamageRange minimum cannot be negative.");
        }

        if (max < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(max),
                max,
                "DamageRange maximum cannot be negative.");
        }

        if (min > max)
        {
            throw new ArgumentException(
                "DamageRange minimum cannot be greater than maximum.");
        }

        Min = min;
        Max = max;
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
                "DamageRange values must be finite.");
        }
    }
}