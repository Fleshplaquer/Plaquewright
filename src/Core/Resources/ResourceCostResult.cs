namespace Plaquewright.Core.Resources;

public readonly record struct ResourceCostResult
{
    public ResourceId ResourceId { get; }

    public double RequestedCost { get; }

    public double ActualCost { get; }

    internal ResourceCostResult(
    ResourceId resourceId,
    double requestedCost,
    double actualCost)
    {
        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        if (!double.IsFinite(
                requestedCost))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCost),
                requestedCost,
                "Resource cost must be finite.");
        }

        if (requestedCost < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCost),
                requestedCost,
                "Resource cost cannot be negative.");
        }

        if (!double.IsFinite(
                actualCost))
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualCost),
                actualCost,
                "Actual resource cost must be finite.");
        }

        if (actualCost < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualCost),
                actualCost,
                "Actual resource cost cannot be negative.");
        }

        if (actualCost >
            requestedCost)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualCost),
                actualCost,
                "Actual resource cost cannot exceed the requested cost.");
        }

        ResourceId =
            resourceId;

        RequestedCost =
            NormalizeZero(
                requestedCost);

        ActualCost =
            NormalizeZero(
                actualCost);
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}