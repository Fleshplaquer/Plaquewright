namespace Plaquewright.Core.Combat;

using Plaquewright.Core.Numerics;
public sealed class HitScopedProtectionBudgetFinancingPlan
{
    public HitScopedProtectionBudget Budget { get; }

    public HitExecutionId HitExecutionId =>
        Budget.HitExecutionId;

    public double AssignedDamage { get; }

    public double BudgetUnitsPerDamage { get; }

    public double RequestedBudgetUnits { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy { get; }

    public HitScopedProtectionBudgetFinancingPlan(
        HitScopedProtectionBudget budget,
        double assignedDamage,
        double budgetUnitsPerDamage,
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        ArgumentNullException.ThrowIfNull(
            budget);

        ValidateAssignedDamage(
            assignedDamage);

        ValidateBudgetUnitsPerDamage(
            budgetUnitsPerDamage);

        ValidateShortfallPolicy(
            shortfallPolicy);

        var requestedBudgetUnits =
            assignedDamage *
            budgetUnitsPerDamage;

        if (!double.IsFinite(
                requestedBudgetUnits))
        {
            throw new OverflowException(
                "Requested protection budget units exceed the finite numeric range.");
        }

        Budget =
            budget;

        AssignedDamage =
            NormalizeZero(
                assignedDamage);

        BudgetUnitsPerDamage =
            budgetUnitsPerDamage;

        RequestedBudgetUnits =
            NormalizeZero(
                requestedBudgetUnits);

        ShortfallPolicy =
            shortfallPolicy;
    }

    public HitScopedProtectionBudgetFinancingResult Reserve()
    {
        var reservation =
            Budget.Reserve(
                HitExecutionId,
                RequestedBudgetUnits);

        return CreateResult(
            reservation);
    }

    internal HitScopedProtectionBudgetFinancingResult ReservePending(
    out HitScopedProtectionBudgetReservationLease reservationLease)
    {
        reservationLease =
            Budget.ReservePending(
                HitExecutionId,
                RequestedBudgetUnits);

        try
        {
            return CreateResult(
                reservationLease.Reservation);
        }
        catch
        {
            reservationLease.Abort();

            throw;
        }
    }

    private HitScopedProtectionBudgetFinancingResult CreateResult(
    HitScopedProtectionBudgetReservation reservation)
    {
        var financedDamage =
            Math.Min(
                AssignedDamage,
                reservation.Reserved /
                BudgetUnitsPerDamage);

        if (NumericComparison.AreEquivalent(
                financedDamage,
                AssignedDamage,
                scale: AssignedDamage))
        {
            financedDamage =
                AssignedDamage;
        }

        var unfinancedDamage =
            Math.Max(
                0d,
                AssignedDamage -
                financedDamage);

        return new HitScopedProtectionBudgetFinancingResult(
            this,
            reservation,
            financedDamage,
            unfinancedDamage);
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

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}