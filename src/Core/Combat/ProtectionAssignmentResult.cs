namespace Idler.Core.Combat;

public sealed class ProtectionAssignmentResult
{
    public ProtectionAssignmentRequest Request { get; }

    public double RequestedFraction =>
        Request.RequestedFraction;

    public double AppliedFraction { get; }

    public double AssignedDamage { get; }

    internal ProtectionAssignmentResult(
        ProtectionAssignmentRequest request,
        double appliedFraction,
        double assignedDamage)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        if (!double.IsFinite(
                appliedFraction) ||
            appliedFraction < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(appliedFraction),
                appliedFraction,
                "Applied protection assignment fraction must be finite and non-negative.");
        }

        if (!double.IsFinite(
                assignedDamage) ||
            assignedDamage < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(assignedDamage),
                assignedDamage,
                "Assigned damage must be finite and non-negative.");
        }

        Request =
            request;

        AppliedFraction =
            appliedFraction == 0d
                ? 0d
                : appliedFraction;

        AssignedDamage =
            assignedDamage == 0d
                ? 0d
                : assignedDamage;
    }
}