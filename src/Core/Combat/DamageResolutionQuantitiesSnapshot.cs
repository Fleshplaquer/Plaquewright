namespace Plaquewright.Core.Combat;

internal readonly record struct
    DamageResolutionQuantitiesSnapshot
{
    public IncomingDamage Incoming { get; }

    public PostMitigationDamage PostMitigation { get; }

    public PostTakenScalingDamage PostTakenScaling { get; }

    public DamageTaken Taken { get; }

    private DamageResolutionQuantitiesSnapshot(
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

    public static DamageResolutionQuantitiesSnapshot Capture(
        DamageResolutionQuantities quantities)
    {
        ArgumentNullException.ThrowIfNull(
            quantities);

        return new DamageResolutionQuantitiesSnapshot(
            quantities.Incoming,
            quantities.PostMitigation,
            quantities.PostTakenScaling,
            quantities.Taken);
    }

    public DamageResolutionQuantities Restore()
    {
        return new DamageResolutionQuantities(
            Incoming,
            PostMitigation,
            PostTakenScaling,
            Taken);
    }
}