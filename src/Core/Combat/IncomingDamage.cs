namespace Idler.Core.Combat;

public readonly record struct IncomingDamage
{
    public double Amount { get; }

    public IncomingDamage(
        double amount)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Incoming damage must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Incoming damage cannot be negative.");
        }

        Amount =
            NormalizeZero(
                amount);
    }

    internal PostMitigationDamage AdvanceToPostMitigation(
        double resolvedAmount)
    {
        return new PostMitigationDamage(
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