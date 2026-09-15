namespace Plaquewright.Core.Combat;

internal sealed class HitScopedProtectionBudgetReservationLease
{
    private readonly HitScopedProtectionBudget _budget;

    private readonly double _reservedCapacityBefore;

    private bool _isFinalized;

    public HitScopedProtectionBudgetReservation Reservation { get; }

    public bool IsFinalized =>
        _isFinalized;

    internal HitScopedProtectionBudgetReservationLease(
        HitScopedProtectionBudget budget,
        HitScopedProtectionBudgetReservation reservation,
        double reservedCapacityBefore)
    {
        ArgumentNullException.ThrowIfNull(
            budget);

        _budget =
            budget;

        Reservation =
            reservation;

        _reservedCapacityBefore =
            reservedCapacityBefore;
    }

    public void Commit()
    {
        EnsureNotFinalized();

        _budget.CommitPendingReservation(
            this);

        _isFinalized =
            true;
    }

    public void Abort()
    {
        EnsureNotFinalized();

        _budget.AbortPendingReservation(
            this,
            _reservedCapacityBefore);

        _isFinalized =
            true;
    }

    private void EnsureNotFinalized()
    {
        if (_isFinalized)
        {
            throw new InvalidOperationException(
                "Protection budget reservation has already been finalized.");
        }
    }
}