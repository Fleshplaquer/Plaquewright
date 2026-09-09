namespace Idler.Core.Combat;

public readonly record struct HitScopedProtectionBudgetReservation
{
    public double Requested { get; }

    public double Reserved { get; }

    public double Shortfall { get; }

    internal HitScopedProtectionBudgetReservation(
        double requested,
        double reserved,
        double shortfall)
    {
        Requested =
            NormalizeZero(
                requested);

        Reserved =
            NormalizeZero(
                reserved);

        Shortfall =
            NormalizeZero(
                shortfall);
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}