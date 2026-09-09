namespace Idler.Core.Combat;

public sealed class ProtectionAssignmentResolution
{
    public double InitialDamage { get; }

    public double RequestedFractionTotal { get; }

    public double AppliedFractionTotal { get; }

    public double AssignmentScale { get; }

    public double PrimaryPathDamage { get; }

    public double TotalAssignedDamage { get; }

    public bool WasScaled =>
        AssignmentScale < 1d;

    public IReadOnlyList<ProtectionAssignmentResult> Assignments { get; }

    public double AccountedDamage =>
        PrimaryPathDamage +
        TotalAssignedDamage;

    internal ProtectionAssignmentResolution(
        double initialDamage,
        double requestedFractionTotal,
        double appliedFractionTotal,
        double assignmentScale,
        double primaryPathDamage,
        ProtectionAssignmentResult[] assignments)
    {
        ArgumentNullException.ThrowIfNull(
            assignments);

        InitialDamage =
            initialDamage;

        RequestedFractionTotal =
            requestedFractionTotal;

        AppliedFractionTotal =
            appliedFractionTotal;

        AssignmentScale =
            assignmentScale;

        PrimaryPathDamage =
            primaryPathDamage == 0d
                ? 0d
                : primaryPathDamage;

        TotalAssignedDamage =
            initialDamage -
            PrimaryPathDamage;

        Assignments =
            Array.AsReadOnly(
                assignments);
    }
}