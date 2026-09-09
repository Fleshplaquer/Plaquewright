namespace Idler.Core.Combat;

public sealed class HitScopedProtectionBudgetRouteExecutionResult
{
    public ProtectionAssignmentRoutingState PreviousState { get; }

    public ProtectionAssignmentRoutingState UpdatedState { get; }

    public HitScopedProtectionBudgetRouteBinding Binding { get; }

    public HitScopedProtectionBudgetFinancingResult FinancingResult { get; }

    public ProtectionShortfallRoutingResult ShortfallRouting { get; }

    public ProtectionAssignmentResult Assignment =>
        Binding.Assignment;

    public HitExecutionId HitExecutionId =>
        Binding.HitExecutionId;

    public double FinancedDamage =>
        FinancingResult.FinancedDamage;

    public double UnfinancedDamage =>
        FinancingResult.UnfinancedDamage;

    public double SpillBackDamage =>
        ShortfallRouting.SpillBackDamage;

    public double ContinueRoutingDamage =>
        ShortfallRouting.ContinueRoutingDamage;

    internal HitScopedProtectionBudgetRouteExecutionResult(
        ProtectionAssignmentRoutingState previousState,
        ProtectionAssignmentRoutingState updatedState,
        HitScopedProtectionBudgetRouteBinding binding,
        HitScopedProtectionBudgetFinancingResult financingResult,
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