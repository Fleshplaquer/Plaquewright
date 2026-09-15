namespace Plaquewright.Core.Combat;

public sealed class ProtectionFinancingResult
{
    public ProtectionFinancingPlan Plan { get; }

    public double AssignedDamage =>
        Plan.AssignedDamage;

    public double RequestedResourceUnits =>
        Plan.RequestedResourceUnits;

    public double ActualResourceUnitsSpent { get; }

    public double ResourceUnitShortfall { get; }

    public double FinancedDamage { get; }

    public double UnfinancedDamage { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy =>
        Plan.ShortfallPolicy;

    internal ProtectionFinancingResult(
        ProtectionFinancingPlan plan,
        double actualResourceUnitsSpent,
        double financedDamage)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        Plan =
            plan;

        ActualResourceUnitsSpent =
            NormalizeZero(
                actualResourceUnitsSpent);

        ResourceUnitShortfall =
            NormalizeZero(
                plan.RequestedResourceUnits -
                actualResourceUnitsSpent);

        FinancedDamage =
            NormalizeZero(
                financedDamage);

        UnfinancedDamage =
            NormalizeZero(
                plan.AssignedDamage -
                financedDamage);
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}