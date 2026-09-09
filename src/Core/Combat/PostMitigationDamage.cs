namespace Idler.Core.Combat;

public readonly record struct PostMitigationDamage
{
    public double Amount { get; }

    internal PostMitigationDamage(
        double amount)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Post-mitigation damage must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Post-mitigation damage cannot be negative.");
        }

        Amount =
            NormalizeZero(
                amount);
    }

    internal PostTakenScalingDamage AdvanceToPostTakenScaling(
        double resolvedAmount)
    {
        return new PostTakenScalingDamage(
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