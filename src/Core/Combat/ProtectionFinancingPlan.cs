namespace Idler.Core.Combat;

public sealed class ProtectionFinancingPlan
{
    public double AssignedDamage { get; }

    public double ResourceUnitsPerDamage { get; }

    public double RequestedResourceUnits { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy { get; }

    public ProtectionFinancingPlan(
        double assignedDamage,
        double resourceUnitsPerDamage,
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        ValidateAssignedDamage(
            assignedDamage);

        ValidateResourceUnitsPerDamage(
            resourceUnitsPerDamage);

        ValidateShortfallPolicy(
            shortfallPolicy);

        var requestedResourceUnits =
            assignedDamage *
            resourceUnitsPerDamage;

        if (!double.IsFinite(
                requestedResourceUnits))
        {
            throw new OverflowException(
                "Protection financing request exceeds the finite numeric range.");
        }

        AssignedDamage =
            NormalizeZero(
                assignedDamage);

        ResourceUnitsPerDamage =
            resourceUnitsPerDamage;

        RequestedResourceUnits =
            NormalizeZero(
                requestedResourceUnits);

        ShortfallPolicy =
            shortfallPolicy;
    }

    public ProtectionFinancingResult Resolve(
        double actualResourceUnitsSpent)
    {
        if (!double.IsFinite(
                actualResourceUnitsSpent))
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualResourceUnitsSpent),
                actualResourceUnitsSpent,
                "Actual resource units spent must be finite.");
        }

        if (actualResourceUnitsSpent < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualResourceUnitsSpent),
                actualResourceUnitsSpent,
                "Actual resource units spent cannot be negative.");
        }

        if (actualResourceUnitsSpent >
            RequestedResourceUnits)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualResourceUnitsSpent),
                actualResourceUnitsSpent,
                "Actual resource units spent cannot exceed requested resource units.");
        }

        var financedDamage =
            Math.Min(
                AssignedDamage,
                actualResourceUnitsSpent /
                ResourceUnitsPerDamage);

        return new ProtectionFinancingResult(
            this,
            actualResourceUnitsSpent,
            financedDamage);
    }

    private static void ValidateAssignedDamage(
        double assignedDamage)
    {
        if (!double.IsFinite(
                assignedDamage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(assignedDamage),
                assignedDamage,
                "Assigned damage must be finite.");
        }

        if (assignedDamage < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(assignedDamage),
                assignedDamage,
                "Assigned damage cannot be negative.");
        }
    }

    private static void ValidateResourceUnitsPerDamage(
        double resourceUnitsPerDamage)
    {
        if (!double.IsFinite(
                resourceUnitsPerDamage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(resourceUnitsPerDamage),
                resourceUnitsPerDamage,
                "Resource units per damage must be finite.");
        }

        if (resourceUnitsPerDamage <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(resourceUnitsPerDamage),
                resourceUnitsPerDamage,
                "Resource units per damage must be greater than zero.");
        }
    }

    private static void ValidateShortfallPolicy(
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        if (shortfallPolicy !=
                ProtectionFinancingShortfallPolicy.SpillBack &&
            shortfallPolicy !=
                ProtectionFinancingShortfallPolicy.ContinueRouting)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shortfallPolicy),
                shortfallPolicy,
                "Unknown protection financing shortfall policy.");
        }
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}