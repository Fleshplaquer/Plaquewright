namespace Idler.Core.Combat;

public sealed class HitScopedProtectionBudget
{
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