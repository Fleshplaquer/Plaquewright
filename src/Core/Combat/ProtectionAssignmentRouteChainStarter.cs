namespace Plaquewright.Core.Combat;

internal static class ProtectionAssignmentRouteChainStarter
{
    public static ProtectionRouteChainState Start(
        ProtectionAssignmentResolution assignmentResolution)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        if (assignmentResolution.InitialDamage == 0d)
        {
            return ProtectionRouteChainState.Start(
                primaryPathDamage: 0d,
                protectionRoutingDamage: 0d);
        }

        ProtectionAssignmentResult? activeAssignment =
            null;

        foreach (var assignment in
                 assignmentResolution.Assignments)
        {
            if (assignment.AssignedDamage == 0d)
            {
                continue;
            }

            if (activeAssignment is not null)
            {
                throw new InvalidOperationException(
                    "A single protection route chain cannot represent multiple active protection assignments.");
            }

            activeAssignment =
                assignment;
        }

        var protectionRoutingDamage =
            activeAssignment?.AssignedDamage ??
            0d;

        if (protectionRoutingDamage !=
            assignmentResolution.TotalAssignedDamage)
        {
            throw new InvalidOperationException(
                "Protection assignment resolution cannot be represented by a single protection route chain.");
        }

        return ProtectionRouteChainState.Start(
            assignmentResolution.PrimaryPathDamage,
            protectionRoutingDamage);
    }
}