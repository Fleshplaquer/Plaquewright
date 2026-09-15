namespace Plaquewright.Core.Combat;

public sealed class ProtectionResourceRouteExecutionResult
{
    public ProtectionAssignmentRoutingState PreviousState { get; }

    public ProtectionAssignmentRoutingState UpdatedState { get; }

    public ProtectionResourceRouteBinding Binding { get; }

    public ResourceBackedProtectionFinancingResult FinancingResult { get; }

    public ProtectionShortfallRoutingResult ShortfallRouting { get; }

    public ProtectionAssignmentResult Assignment =>
        Binding.Assignment;

    public double FinancedDamage =>
        FinancingResult.FinancedDamage;

    public double UnfinancedDamage =>
        FinancingResult.UnfinancedDamage;

    public double SpillBackDamage =>
        ShortfallRouting.SpillBackDamage;

    public double ContinueRoutingDamage =>
        ShortfallRouting.ContinueRoutingDamage;

    internal ProtectionResourceRouteExecutionResult(
        ProtectionAssignmentRoutingState previousState,
        ProtectionAssignmentRoutingState updatedState,
        ProtectionResourceRouteBinding binding,
        ResourceBackedProtectionFinancingResult financingResult,
        ProtectionShortfallRoutingResult shortfallRouting)
    {
        ArgumentNullException.ThrowIfNull(
            previousState);

        ArgumentNullException.ThrowIfNull(
            updatedState);

        ArgumentNullException.ThrowIfNull(
            binding);

        ArgumentNullException.ThrowIfNull(
            financingResult);

        ArgumentNullException.ThrowIfNull(
            shortfallRouting);

        PreviousState =
            previousState;

        UpdatedState =
            updatedState;

        Binding =
            binding;

        FinancingResult =
            financingResult;

        ShortfallRouting =
            shortfallRouting;
    }
}