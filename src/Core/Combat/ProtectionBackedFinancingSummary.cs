namespace Plaquewright.Core.Combat;

using Plaquewright.Core.Numerics;

public sealed class ProtectionBackedFinancingSummary
{
    public double AssignedDamage { get; }

    public double FinancedDamage { get; }

    public double UnfinancedDamage { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy { get; }

    internal ProtectionBackedFinancingSummary(
        double assignedDamage,
        double financedDamage,
        double unfinancedDamage,
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        ValidateDamage(
            assignedDamage,
            nameof(assignedDamage));

        ValidateDamage(
            financedDamage,
            nameof(financedDamage));

        ValidateDamage(
            unfinancedDamage,
            nameof(unfinancedDamage));

        var accountedDamage =
    financedDamage +
    unfinancedDamage;

        if (!NumericComparison.AreEquivalent(
                accountedDamage,
                assignedDamage,
                scale: assignedDamage))
        {
            throw new InvalidOperationException(
                "Protection financing must account for all assigned damage.");
        }

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

        AssignedDamage =
            NormalizeZero(
                assignedDamage);

        FinancedDamage =
            NormalizeZero(
                financedDamage);

        UnfinancedDamage =
            NormalizeZero(
                unfinancedDamage);

        ShortfallPolicy =
            shortfallPolicy;
    }

    internal static ProtectionBackedFinancingSummary From(
        ProtectionFinancingResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        return new ProtectionBackedFinancingSummary(
            result.AssignedDamage,
            result.FinancedDamage,
            result.UnfinancedDamage,
            result.ShortfallPolicy);
    }

    internal static ProtectionBackedFinancingSummary From(
        ResourceBackedProtectionFinancingResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        return new ProtectionBackedFinancingSummary(
            result.AssignedDamage,
            result.FinancedDamage,
            result.UnfinancedDamage,
            result.ShortfallPolicy);
    }

    internal static ProtectionBackedFinancingSummary From(
        HitScopedProtectionBudgetFinancingResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        return new ProtectionBackedFinancingSummary(
            result.AssignedDamage,
            result.FinancedDamage,
            result.UnfinancedDamage,
            result.ShortfallPolicy);
    }

    private static void ValidateDamage(
        double amount,
        string parameterName)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                amount,
                "Damage amount must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                amount,
                "Damage amount cannot be negative.");
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