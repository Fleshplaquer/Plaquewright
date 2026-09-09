namespace Idler.Core.Combat;

public sealed class ProtectionShortfallRoutingResult
{
    public ProtectionBackedFinancingSummary Financing { get; }

    public double UnfinancedDamage =>
        Financing.UnfinancedDamage;

    public ProtectionFinancingShortfallPolicy Policy =>
        Financing.ShortfallPolicy;

    public double SpillBackDamage { get; }

    public double ContinueRoutingDamage { get; }

    internal ProtectionShortfallRoutingResult(
        ProtectionBackedFinancingSummary financing,
        double spillBackDamage,
        double continueRoutingDamage)
    {
        ArgumentNullException.ThrowIfNull(
            financing);

        ValidateDamage(
            spillBackDamage,
            nameof(spillBackDamage));

        ValidateDamage(
            continueRoutingDamage,
            nameof(continueRoutingDamage));

        var routedDamage =
            spillBackDamage +
            continueRoutingDamage;

        if (!double.IsFinite(
                routedDamage))
        {
            throw new OverflowException(
                "Protection shortfall routing exceeds the finite numeric range.");
        }

        if (routedDamage !=
            financing.UnfinancedDamage)
        {
            throw new InvalidOperationException(
                "Protection shortfall routing must account for all unfinanced damage.");
        }

        Financing =
            financing;

        SpillBackDamage =
            NormalizeZero(
                spillBackDamage);

        ContinueRoutingDamage =
            NormalizeZero(
                continueRoutingDamage);
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
                "Routed damage must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                amount,
                "Routed damage cannot be negative.");
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