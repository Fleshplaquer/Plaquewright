namespace Idler.Core.Combat;

public sealed class DamageResolutionQuantities
{
    public IncomingDamage Incoming { get; }

    public PostMitigationDamage PostMitigation { get; }

    public PostTakenScalingDamage PostTakenScaling { get; }

    public DamageTaken Taken { get; }

    internal DamageResolutionQuantities(
        IncomingDamage incoming,
        PostMitigationDamage postMitigation,
        PostTakenScalingDamage postTakenScaling,
        DamageTaken taken)
    {
        Incoming =
            incoming;

        PostMitigation =
            postMitigation;

        PostTakenScaling =
            postTakenScaling;

        Taken =
            taken;
    }

    internal static DamageResolutionQuantities Create(
        IncomingDamage incoming,
        double postMitigationAmount,
        double postTakenScalingAmount,
        double damageTakenAmount)
    {
        var postMitigation =
            incoming.AdvanceToPostMitigation(
                postMitigationAmount);

        var postTakenScaling =
            postMitigation.AdvanceToPostTakenScaling(
                postTakenScalingAmount);

        var taken =
            postTakenScaling.AdvanceToDamageTaken(
                damageTakenAmount);

        return new DamageResolutionQuantities(
            incoming,
            postMitigation,
            postTakenScaling,
            taken);
    }
}