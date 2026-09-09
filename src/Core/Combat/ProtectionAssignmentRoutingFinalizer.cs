namespace Idler.Core.Combat;

internal static class ProtectionAssignmentRoutingFinalizer
{
    public static ProtectionAssignmentRoutingState Finalize(
        ProtectionAssignmentRoutingState state)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        return state.FinalizeUnresolvedToPrimaryPath();
    }
}