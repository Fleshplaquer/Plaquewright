namespace Plaquewright.Core.Combat;

public sealed class HitScopedProtectionBudget
{

    private HitScopedProtectionBudgetReservationLease?
    _pendingReservation;
    public HitExecutionId HitExecutionId { get; }

    public double InitialCapacity { get; }

    public double ReservedCapacity { get; private set; }

    public double RemainingCapacity =>
        NormalizeZero(
            InitialCapacity -
            ReservedCapacity);

    public bool IsExhausted =>
        RemainingCapacity == 0d;

    public HitScopedProtectionBudget(
        HitExecutionId hitExecutionId,
        double initialCapacity)
    {
        if (hitExecutionId == default)
        {
            throw new ArgumentException(
                "Hit execution ID must be valid.",
                nameof(hitExecutionId));
        }

        ValidateCapacity(
            initialCapacity,
            nameof(initialCapacity));

        HitExecutionId =
            hitExecutionId;

        InitialCapacity =
            NormalizeZero(
                initialCapacity);

        ReservedCapacity =
            0d;
    }

    public HitScopedProtectionBudgetReservation Reserve(
    HitExecutionId hitExecutionId,
    double requestedCapacity)
    {
        EnsureNoPendingReservation();

        return ReserveCore(
            hitExecutionId,
            requestedCapacity);
    }

    internal HitScopedProtectionBudgetReservationLease ReservePending(
    HitExecutionId hitExecutionId,
    double requestedCapacity)
    {
        EnsureNoPendingReservation();

        var reservedCapacityBefore =
            ReservedCapacity;

        var reservation =
            ReserveCore(
                hitExecutionId,
                requestedCapacity);

        var lease =
            new HitScopedProtectionBudgetReservationLease(
                this,
                reservation,
                reservedCapacityBefore);

        _pendingReservation =
            lease;

        return lease;
    }

    internal void CommitPendingReservation(
        HitScopedProtectionBudgetReservationLease reservation)
    {
        ArgumentNullException.ThrowIfNull(
            reservation);

        ValidatePendingReservation(
            reservation);

        _pendingReservation =
            null;
    }

    internal void AbortPendingReservation(
        HitScopedProtectionBudgetReservationLease reservation,
        double reservedCapacityBefore)
    {
        ArgumentNullException.ThrowIfNull(
            reservation);

        ValidatePendingReservation(
            reservation);

        ReservedCapacity =
            NormalizeZero(
                reservedCapacityBefore);

        _pendingReservation =
            null;
    }

    private HitScopedProtectionBudgetReservation ReserveCore(
    HitExecutionId hitExecutionId,
    double requestedCapacity)
    {
        if (hitExecutionId !=
            HitExecutionId)
        {
            throw new InvalidOperationException(
                "Protection budget belongs to a different hit execution.");
        }

        ValidateCapacity(
            requestedCapacity,
            nameof(requestedCapacity));

        var reserved =
            Math.Min(
                requestedCapacity,
                RemainingCapacity);

        var shortfall =
            requestedCapacity -
            reserved;

        var nextReservedCapacity =
            ReservedCapacity +
            reserved;

        if (!double.IsFinite(
                nextReservedCapacity))
        {
            throw new OverflowException(
                "Protection budget reservation exceeds the finite numeric range.");
        }

        ReservedCapacity =
            NormalizeZero(
                nextReservedCapacity);

        return new HitScopedProtectionBudgetReservation(
            requestedCapacity,
            reserved,
            shortfall);
    }

    private void EnsureNoPendingReservation()
    {
        if (_pendingReservation is not null)
        {
            throw new InvalidOperationException(
                "Protection budget already has a pending reservation.");
        }
    }

    private void ValidatePendingReservation(
        HitScopedProtectionBudgetReservationLease reservation)
    {
        if (!ReferenceEquals(
                _pendingReservation,
                reservation))
        {
            throw new InvalidOperationException(
                "Protection budget reservation does not belong to the active pending reservation.");
        }
    }

    private static void ValidateCapacity(
        double capacity,
        string parameterName)
    {
        if (!double.IsFinite(
                capacity))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                capacity,
                "Protection budget capacity must be finite.");
        }

        if (capacity < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                capacity,
                "Protection budget capacity cannot be negative.");
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