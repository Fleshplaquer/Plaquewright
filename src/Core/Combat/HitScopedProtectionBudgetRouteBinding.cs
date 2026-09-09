namespace Idler.Core.Combat;

public sealed class HitScopedProtectionBudgetRouteBinding
{
    public ProtectionAssignmentResult Assignment { get; }

    public HitScopedProtectionBudget Budget { get; }

    public HitExecutionId HitExecutionId =>
        Budget.HitExecutionId;

    public double BudgetUnitsPerDamage { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy { get; }

    public HitScopedProtectionBudgetRouteBinding(
        ProtectionAssignmentResult assignment,
        HitScopedProtectionBudget budget,
        double budgetUnitsPerDamage,
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        ArgumentNullException.ThrowIfNull(
            assignment);

        ArgumentNullException.ThrowIfNull(
            budget);

        ValidateBudgetUnitsPerDamage(
            budgetUnitsPerDamage);

        ValidateShortfallPolicy(
            shortfallPolicy);

        Assignment =
            assignment;

        Budget =
            budget;

        BudgetUnitsPerDamage =
            budgetUnitsPerDamage;

        ShortfallPolicy =
            shortfallPolicy;
    }

    internal HitScopedProtectionBudgetFinancingPlan CreateFinancingPlan(
        ProtectionAssignmentLaneState lane)
    {
        ArgumentNullException.ThrowIfNull(
            lane);

        if (!ReferenceEquals(
                lane.Assignment,
                Assignment))
        {
            throw new InvalidOperationException(
                "Hit-scoped protection budget route binding belongs to a different assignment lane.");
        }

        if (lane.IsComplete)
        {
            throw new InvalidOperationException(
                "Cannot create hit-scoped protection financing for a completed assignment lane.");
        }

        return new HitScopedProtectionBudgetFinancingPlan(
            Budget,
            assignedDamage:
                lane.ContinueRoutingDamage,
            budgetUnitsPerDamage:
                BudgetUnitsPerDamage,
            ShortfallPolicy);
    }

    private static void ValidateBudgetUnitsPerDamage(
        double budgetUnitsPerDamage)
    {
        if (!double.IsFinite(
                budgetUnitsPerDamage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(budgetUnitsPerDamage),
                budgetUnitsPerDamage,
                "Budget units per damage must be finite.");
        }

        if (budgetUnitsPerDamage <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(budgetUnitsPerDamage),
                budgetUnitsPerDamage,
                "Budget units per damage must be greater than zero.");
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
}