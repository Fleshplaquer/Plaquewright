namespace Plaquewright.Core.Simulation;

internal readonly record struct HitExecutionIdAllocatorSnapshot
{
    public ulong NextValue { get; }

    public HitExecutionIdAllocatorSnapshot(
        ulong nextValue)
    {
        NextValue =
            nextValue;
    }
}