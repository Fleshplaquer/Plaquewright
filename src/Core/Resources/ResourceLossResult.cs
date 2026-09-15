namespace Plaquewright.Core.Resources;

public readonly record struct ResourceLossResult
{
    public ResourceId ResourceId { get; }

    public double RequestedLoss { get; }

    public double PreventedLoss { get; }

    public double ActualLoss { get; }

    public double Shortfall { get; }

    internal ResourceLossResult(
        ResourceId resourceId,
        double requestedLoss,
        double preventedLoss,
        double actualLoss)
    {
        if (!resourceId.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(resourceId));
        }

        ValidateNonNegativeFinite(
            requestedLoss,
            nameof(requestedLoss));

        ValidateNonNegativeFinite(
            preventedLoss,
            nameof(preventedLoss));

        ValidateNonNegativeFinite(
            actualLoss,
            nameof(actualLoss));

        if (preventedLoss > requestedLoss)
        {
            throw new ArgumentOutOfRangeException(
                nameof(preventedLoss),
                preventedLoss,
                "Prevented loss cannot exceed requested loss.");
        }

        var remainingAfterPrevention =
            requestedLoss - preventedLoss;

        if (actualLoss > remainingAfterPrevention)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actualLoss),
                actualLoss,
                "Actual loss cannot exceed loss remaining after prevention.");
        }

        ResourceId = resourceId;
        RequestedLoss = NormalizeZero(requestedLoss);
        PreventedLoss = NormalizeZero(preventedLoss);
        ActualLoss = NormalizeZero(actualLoss);

        Shortfall =
            NormalizeZero(
                remainingAfterPrevention -
                actualLoss);
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