namespace Plaquewright.Core.Simulation;

internal readonly record struct DamageExecutionIdAllocatorSnapshot
{
    public ulong NextValue { get; }

    public DamageExecutionIdAllocatorSnapshot(
        ulong nextValue)
    {
        NextValue =
            nextValue;
    }
}