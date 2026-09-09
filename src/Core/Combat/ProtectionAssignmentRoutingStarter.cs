namespace Idler.Core.Combat;

internal static class ProtectionAssignmentRoutingStarter
{
    public static ProtectionAssignmentRoutingState Start(
        ProtectionAssignmentResolution assignmentResolution)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        return ProtectionAssignmentRoutingState.Start(
            assignmentResolution);
    }

    public static ProtectionAssignmentRoutingState Start(
        ProtectionAssignmentResolution assignmentResolution,
        DamageTargetContext damageTarget)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        ArgumentNullException.ThrowIfNull(
            damageTarget);

        return ProtectionAssignmentRoutingState.Start(
            assignmentResolution,
            damageTarget);
    }
}