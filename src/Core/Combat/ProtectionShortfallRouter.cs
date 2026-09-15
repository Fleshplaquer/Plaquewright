namespace Plaquewright.Core.Combat;

internal static class ProtectionShortfallRouter
{
    public static ProtectionShortfallRoutingResult Route(
        ProtectionFinancingResult financingResult)
    {
        ArgumentNullException.ThrowIfNull(
            financingResult);

        return Route(
            ProtectionBackedFinancingSummary.From(
                financingResult));
    }

    public static ProtectionShortfallRoutingResult Route(
        ResourceBackedProtectionFinancingResult financingResult)
    {
        ArgumentNullException.ThrowIfNull(
            financingResult);

        return Route(
            ProtectionBackedFinancingSummary.From(
                financingResult));
    }

    public static ProtectionShortfallRoutingResult Route(
        HitScopedProtectionBudgetFinancingResult financingResult)
    {
        ArgumentNullException.ThrowIfNull(
            financingResult);

        return Route(
            ProtectionBackedFinancingSummary.From(
                financingResult));
    }

    private static ProtectionShortfallRoutingResult Route(
        ProtectionBackedFinancingSummary financing)
    {
        return financing.ShortfallPolicy switch
        {
            ProtectionFinancingShortfallPolicy.SpillBack =>
                new ProtectionShortfallRoutingResult(
                    financing,
                    spillBackDamage:
                        financing.UnfinancedDamage,
                    continueRoutingDamage:
                        0d),

            ProtectionFinancingShortfallPolicy.ContinueRouting =>
                new ProtectionShortfallRoutingResult(
                    financing,
                    spillBackDamage:
                        0d,
                    continueRoutingDamage:
                        financing.UnfinancedDamage),

            _ =>
                throw new InvalidOperationException(
                    "Unsupported protection financing shortfall policy.")
        };
    }
}