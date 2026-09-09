namespace Idler.Core.Combat;

public readonly record struct DamageTaken
{
    public double Amount { get; }

    internal DamageTaken(
        double amount)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Damage taken must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Damage taken cannot be negative.");
        }

        Amount =
            NormalizeZero(
                amount);
    }

    internal ActualResourceLoss AdvanceToActualResourceLoss(
        double resolvedAmount)
    {
        return new ActualResourceLoss(
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