using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class ProtectionResourceRouteBatchItemResult
{
    public ProtectionResourceRouteBinding Binding { get; }

    public ProtectionFinancingPlan FinancingPlan { get; }

    public ResourceLossPreview ResourcePreview { get; }

    public ProtectionFinancingResult FinancingResult { get; }

    public ProtectionShortfallRoutingResult ShortfallRouting { get; }

    public ProtectionAssignmentResult Assignment =>
        Binding.Assignment;

    public double RequestedResourceUnits =>
        FinancingPlan.RequestedResourceUnits;

    public double AllocatedResourceUnits =>
        ResourcePreview.Request.Amount;

    public double ActualResourceUnitsSpent =>
        FinancingResult.ActualResourceUnitsSpent;

    public double ResourceUnitShortfall =>
        FinancingResult.ResourceUnitShortfall;

    public double FinancedDamage =>
        FinancingResult.FinancedDamage;

    public double UnfinancedDamage =>
        FinancingResult.UnfinancedDamage;

    public double SpillBackDamage =>
        ShortfallRouting.SpillBackDamage;

    public double ContinueRoutingDamage =>
        ShortfallRouting.ContinueRoutingDamage;

    internal ProtectionResourceRouteBatchItemResult(
        ProtectionResourceRouteBinding binding,
        ProtectionFinancingPlan financingPlan,
        ResourceLossPreview resourcePreview,
        ProtectionFinancingResult financingResult,
        ProtectionShortfallRoutingResult shortfallRouting)
    {
        ArgumentNullException.ThrowIfNull(
            binding);

        ArgumentNullException.ThrowIfNull(
            financingPlan);

        ArgumentNullException.ThrowIfNull(
            resourcePreview);

        ArgumentNullException.ThrowIfNull(
            financingResult);

        ArgumentNullException.ThrowIfNull(
            shortfallRouting);

        Binding =
            binding;

        FinancingPlan =
            financingPlan;

        ResourcePreview =
            resourcePreview;

        FinancingResult =
            financingResult;

        ShortfallRouting =
            shortfallRouting;
    }
}