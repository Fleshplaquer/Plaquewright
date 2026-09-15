namespace Plaquewright.Core.Combat;

public sealed class ProtectionAssignmentRequest
{
    public double RequestedFraction { get; }

    public ProtectionAssignmentRequest(
        double requestedFraction)
    {
        if (!double.IsFinite(
                requestedFraction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedFraction),
                requestedFraction,
                "Requested protection assignment fraction must be finite.");
        }

        if (requestedFraction < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedFraction),
                requestedFraction,
                "Requested protection assignment fraction cannot be negative.");
        }

        RequestedFraction =
            requestedFraction == 0d
                ? 0d
                : requestedFraction;
    }
}