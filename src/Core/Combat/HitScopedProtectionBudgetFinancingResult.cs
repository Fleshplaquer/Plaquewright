namespace Idler.Core.Combat;

public sealed class HitScopedProtectionBudgetFinancingResult
{
    public HitScopedProtectionBudgetFinancingPlan Plan { get; }

    public HitScopedProtectionBudgetReservation Reservation { get; }

    public HitExecutionId HitExecutionId =>
        Plan.HitExecutionId;

    public double AssignedDamage =>
        Plan.AssignedDamage;

    public double RequestedBudgetUnits =>
        Reservation.Requested;

    public double ReservedBudgetUnits =>
        Reservation.Reserved;

    public double BudgetUnitShortfall =>
        Reservation.Shortfall;

    public double FinancedDamage { get; }

    public double UnfinancedDamage { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy =>
        Plan.ShortfallPolicy;

    internal HitScopedProtectionBudgetFinancingResult(
        HitScopedProtectionBudgetFinancingPlan plan,
        HitScopedProtectionBudgetReservation reservation,
        double financedDamage,
        double unfinancedDamage)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        Plan =
            plan;

        Reservation =
            reservation;

        FinancedDamage =
            NormalizeZero(
                financedDamage);

        UnfinancedDamage =
            NormalizeZero(
                unfinancedDamage);
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}