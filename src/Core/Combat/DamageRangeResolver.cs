namespace Idler.Core.Combat;

public static class DamageRangeResolver
{
    public static DamageRange Resolve(
        double candidateMin,
        double candidateMax,
        RangeResolutionPolicy policy =
            RangeResolutionPolicy.CollapseBetween)
    {
        ValidateFinite(candidateMin, nameof(candidateMin));
        ValidateFinite(candidateMax, nameof(candidateMax));

        (var resolvedMin, var resolvedMax) =
            ResolveOrdering(
                candidateMin,
                candidateMax,
                policy);

        // Important:
        // Damage-domain clamping happens AFTER range resolution.
        resolvedMin = Math.Max(0d, resolvedMin);
        resolvedMax = Math.Max(0d, resolvedMax);

        return new DamageRange(
            resolvedMin,
            resolvedMax);
    }

    private static (double Min, double Max) ResolveOrdering(
        double candidateMin,
        double candidateMax,
        RangeResolutionPolicy policy)
    {
        if (policy !=
            RangeResolutionPolicy.CollapseBetween)
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                policy,
                "Unknown range resolution policy.");
        }

        if (candidateMin <= candidateMax)
        {
            return (candidateMin, candidateMax);
        }

        return CollapseBetween(
            candidateMin,
            candidateMax);
    }

    private static (double Min, double Max) CollapseBetween(
        double candidateMin,
        double candidateMax)
    {
        // Avoid (min + max) / 2 to reduce overflow risk.
        var midpoint =
            (candidateMin / 2d) +
            (candidateMax / 2d);

        return (midpoint, midpoint);
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
                "Damage range candidates must be finite.");
        }
    }
}