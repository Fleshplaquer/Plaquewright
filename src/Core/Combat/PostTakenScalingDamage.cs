namespace Idler.Core.Combat;

public readonly record struct PostTakenScalingDamage
{
    public double Amount { get; }

    internal PostTakenScalingDamage(
        double amount)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Post-taken-scaling damage must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Post-taken-scaling damage cannot be negative.");
        }

        Amount =
            NormalizeZero(
                amount);
    }

    internal DamageTaken AdvanceToDamageTaken(
        double resolvedAmount)
    {
        return new DamageTaken(
            resolvedAmount);
    }

    public override string ToString()
    {
        return Amount.ToString();
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}