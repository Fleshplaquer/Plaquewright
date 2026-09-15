namespace Plaquewright.Core.Resources;

public readonly record struct ResourceLossRequest
{
    public ResourceId ResourceId { get; }

    public double Amount { get; }

    public ResourceOperationProvenance Provenance { get; }

    public ResourceLossRequest(
        ResourceId resourceId,
        double amount,
        ResourceOperationProvenance provenance)
    {
        ValidateResourceId(resourceId);
        ValidateAmount(amount);
        ValidateProvenance(provenance);

        ResourceId = resourceId;
        Amount = NormalizeZero(amount);
        Provenance = provenance;
    }

    private static void ValidateResourceId(
        ResourceId resourceId)
    {
        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }
    }

    private static void ValidateAmount(
        double amount)
    {
        if (!double.IsFinite(amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Resource loss amount must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Resource loss amount cannot be negative.");
        }
    }

    private static void ValidateProvenance(
        ResourceOperationProvenance provenance)
    {
        if (!provenance.IsValid)
        {
            throw new ArgumentException(
                "Resource operation provenance must be valid.",
                nameof(provenance));
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