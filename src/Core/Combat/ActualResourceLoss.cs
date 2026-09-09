namespace Idler.Core.Combat;

public readonly record struct ActualResourceLoss
{
    public double Amount { get; }

    internal ActualResourceLoss(
        double amount)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Actual resource loss must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Actual resource loss cannot be negative.");
        }

        Amount =
            NormalizeZero(
                amount);
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