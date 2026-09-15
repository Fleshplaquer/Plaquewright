namespace Plaquewright.Core.Resources;

public readonly record struct ResourceRecoveryResult
{
    public ResourceId ResourceId { get; }

    public double RequestedRecovery { get; }

    public double ActualRecovery { get; }

    public double Overflow { get; }

    internal ResourceRecoveryResult(
        ResourceId resourceId,
        double requestedRecovery,
        double actualRecovery)
    {
        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        ValidateNonNegativeFinite(
            requestedRecovery,
            nameof(requestedRecovery));

        ValidateNonNegativeFinite(
            actualRecovery,
            nameof(actualRecovery));

        if (actualRecovery > requestedRecovery)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualRecovery),
                actualRecovery,
                "Actual recovery cannot exceed requested recovery.");
        }

        ResourceId = resourceId;
        RequestedRecovery =
            NormalizeZero(requestedRecovery);

        ActualRecovery =
            NormalizeZero(actualRecovery);

        Overflow =
            NormalizeZero(
                requestedRecovery -
                actualRecovery);
    }

    private static void ValidateNonNegativeFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity must be finite.");
        }

        if (value < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity cannot be negative.");
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