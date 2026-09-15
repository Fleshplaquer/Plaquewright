namespace Plaquewright.Core.Combat;

public sealed class HitScopedProtectionBudgetRouteBatchItemResult
{
    public HitScopedProtectionBudgetRouteBinding Binding { get; }

    public HitScopedProtectionBudgetFinancingResult FinancingResult { get; }

    public ProtectionShortfallRoutingResult ShortfallRouting { get; }

    public ProtectionAssignmentResult Assignment =>
        Binding.Assignment;

    public double RequestedBudgetUnits =>
        FinancingResult.RequestedBudgetUnits;

    public double AllocatedBudgetUnits =>
        FinancingResult.ReservedBudgetUnits;

    public double BudgetUnitShortfall =>
        FinancingResult.BudgetUnitShortfall;

    public double FinancedDamage =>
        FinancingResult.FinancedDamage;

    public double UnfinancedDamage =>
        FinancingResult.UnfinancedDamage;

    public double SpillBackDamage =>
        ShortfallRouting.SpillBackDamage;

    public double ContinueRoutingDamage =>
        ShortfallRouting.ContinueRoutingDamage;

    internal HitScopedProtectionBudgetRouteBatchItemResult(
        HitScopedProtectionBudgetRouteBinding binding,
        HitScopedProtectionBudgetFinancingResult financingResult,
        ProtectionShortfallRoutingResult shortfallRouting)
    {
        ArgumentNullException.ThrowIfNull(
            binding);

        ArgumentNullException.ThrowIfNull(
            financingResult);

        ArgumentNullException.ThrowIfNull(
            shortfallRouting);

        Binding =
            binding;

        FinancingResult =
            financingResult;

        ShortfallRouting =
            shortfallRouting;
    }
}