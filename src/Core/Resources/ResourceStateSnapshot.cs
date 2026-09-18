namespace Plaquewright.Core.Resources;

internal readonly record struct ResourceStateSnapshot
{
    public ResourceId Id { get; }

    public double Current { get; }

    public double Maximum { get; }

    public ulong Revision { get; }

    private ResourceStateSnapshot(
        ResourceId id,
        double current,
        double maximum,
        ulong revision)
    {
        Id =
            id;

        Current =
            current;

        Maximum =
            maximum;

        Revision =
            revision;
    }

    public static ResourceStateSnapshot Capture(
        ResourceState state)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        return new ResourceStateSnapshot(
            state.Id,
            state.Current,
            state.Maximum,
            state.Revision);
    }

    public ResourceState Restore()
    {
        return new ResourceState(
            Id,
            Current,
            Maximum,
            Revision);
    }
}