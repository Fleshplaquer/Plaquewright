namespace Plaquewright.Core.Combat;

internal static class DamageTakenResolver
{
    public static DamageTaken Resolve(
        PostTakenScalingDamage postTakenScaling,
        ProtectionAssignmentRoutingState protectionRouting)
    {
        ArgumentNullException.ThrowIfNull(
            protectionRouting);

        protectionRouting.ValidateCurrentForTransition();

        if (protectionRouting.InitialDamage !=
            postTakenScaling.Amount)
        {
            throw new InvalidOperationException(
                "Protection routing state does not represent the post-taken-scaling damage being resolved.");
        }

        if (!protectionRouting.IsComplete)
        {
            throw new InvalidOperationException(
                "Damage taken cannot be resolved before general protection routing is complete.");
        }

        return new DamageTaken(
            protectionRouting.PrimaryPathDamage);
    }
}