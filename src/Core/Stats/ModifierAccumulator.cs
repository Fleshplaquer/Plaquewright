namespace Plaquewright.Core.Stats;

public sealed class ModifierAccumulator
{
    private readonly List<double> _flat = [];
    private readonly List<double> _increased = [];
    private readonly List<double> _reduced = [];
    private readonly List<double> _more = [];
    private readonly List<double> _less = [];

    public double Flat =>
        SumDeterministically(_flat);

    public double Increased =>
        SumDeterministically(_increased);

    public double Reduced =>
        SumDeterministically(_reduced);

    public int FlatCount => _flat.Count;

    public int IncreasedCount => _increased.Count;

    public int ReducedCount => _reduced.Count;

    public int MoreCount => _more.Count;

    public int LessCount => _less.Count;

    public void AddFlat(double value)
    {
        ValidateFinite(
            value,
            nameof(value));

        _flat.Add(value);
    }

    public void AddIncreased(double value)
    {
        ValidateNonNegativeFinite(
            value,
            nameof(value));

        _increased.Add(value);
    }

    public void AddReduced(double value)
    {
        ValidateNonNegativeFinite(
            value,
            nameof(value));

        _reduced.Add(value);
    }

    public void AddMore(double value)
    {
        ValidateNonNegativeFinite(
            value,
            nameof(value));

        _more.Add(value);
    }

    public void AddLess(double value)
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

        _less.Add(value);
    }

    public double Apply(double baseValue)
    {
        return ModifierMath.Apply(
            baseValue: baseValue,
            flat: Flat,
            increased: Increased,
            reduced: Reduced,
            more: _more,
            less: _less);
    }

    private static double SumDeterministically(
        IReadOnlyCollection<double> values)
    {
        if (values.Count == 0)
        {
            return 0d;
        }

        var ordered = values
            .OrderBy(value => value)
            .ToArray();

        var result = 0d;

        foreach (var value in ordered)
        {
            result += value;

            if (!double.IsFinite(result))
            {
                throw new OverflowException(
                    "Modifier accumulation produced a non-finite value.");
            }
        }

        return result;
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
                "Modifier value must be finite.");
        }
    }
}