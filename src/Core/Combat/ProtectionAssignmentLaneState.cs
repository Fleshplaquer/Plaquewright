namespace Plaquewright.Core.Combat;

public sealed class ProtectionAssignmentLaneState
{
    public ProtectionAssignmentResult Assignment { get; }

    public ProtectionRouteChainState Chain { get; }

    public double AssignedDamage =>
        Assignment.AssignedDamage;

    public double SpillBackDamage =>
        Chain.PrimaryPathDamage;

    public double ContinueRoutingDamage =>
        Chain.ContinueRoutingDamage;

    public double FinancedDamage =>
        Chain.FinancedDamage;

    public bool IsComplete =>
        Chain.IsComplete;

    internal ProtectionAssignmentLaneState(
        ProtectionAssignmentResult assignment,
        ProtectionRouteChainState chain)
    {
        ArgumentNullException.ThrowIfNull(
            assignment);

        ArgumentNullException.ThrowIfNull(
            chain);

        if (chain.InitialDamage !=
            assignment.AssignedDamage)
        {
            throw new InvalidOperationException(
                "Protection assignment lane chain must represent exactly the damage assigned to that lane.");
        }

        Assignment =
            assignment;

        Chain =
            chain;
    }

    internal static ProtectionAssignmentLaneState Start(
        ProtectionAssignmentResult assignment)
    {
        ArgumentNullException.ThrowIfNull(
            assignment);

        return new ProtectionAssignmentLaneState(
            assignment,
            ProtectionRouteChainState.Start(
                primaryPathDamage: 0d,
                protectionRoutingDamage:
                    assignment.AssignedDamage));
    }

    internal ProtectionAssignmentLaneState Apply(
        ProtectionShortfallRoutingResult routing)
    {
        ArgumentNullException.ThrowIfNull(
            routing);

        if (IsComplete)
        {
            throw new InvalidOperationException(
                "Cannot apply another protection route to a completed assignment lane.");
        }

        return new ProtectionAssignmentLaneState(
            Assignment,
            Chain.Apply(
                routing));
    }
    internal ProtectionAssignmentLaneState FinalizeUnresolvedToPrimaryPath()
    {
        var finalizedChain =
            Chain.FinalizeUnresolvedToPrimaryPath();

        if (ReferenceEquals(
                finalizedChain,
                Chain))
        {
            return this;
        }

        return new ProtectionAssignmentLaneState(
            Assignment,
            finalizedChain);
    }
}