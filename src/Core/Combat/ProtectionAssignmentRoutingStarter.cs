namespace Plaquewright.Core.Combat;

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
    public static ProtectionAssignmentRoutingState Start(
    ProtectionAssignmentResolution assignmentResolution,
    DamageExecutionContext damageExecution,
    DamageTargetContext damageTarget)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        ArgumentNullException.ThrowIfNull(
            damageExecution);

        ArgumentNullException.ThrowIfNull(
            damageTarget);

        return ProtectionAssignmentRoutingState.Start(
            assignmentResolution,
            damageExecution,
            damageTarget);
    }
}