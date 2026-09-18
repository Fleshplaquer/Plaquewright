namespace Plaquewright.Core.Simulation;

internal readonly record struct EntityIdAllocatorSnapshot
{
    public ulong NextValue { get; }

    public bool IsExhausted { get; }

    public EntityIdAllocatorSnapshot(
        ulong nextValue,
        bool isExhausted)
    {
        if (nextValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextValue),
                nextValue,
                "Next entity ID value must be greater than zero.");
        }

        if (isExhausted &&
            nextValue != ulong.MaxValue)
        {
            throw new ArgumentException(
                "An exhausted entity ID allocator must be positioned at ulong.MaxValue.",
                nameof(isExhausted));
        }

        NextValue =
            nextValue;

        IsExhausted =
            isExhausted;
    }
}