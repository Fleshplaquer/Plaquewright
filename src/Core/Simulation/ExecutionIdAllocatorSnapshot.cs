namespace Plaquewright.Core.Simulation;

internal readonly record struct ExecutionIdAllocatorSnapshot
{
    public ulong NextValue { get; }

    public bool IsExhausted { get; }

    public ExecutionIdAllocatorSnapshot(
        ulong nextValue,
        bool isExhausted)
    {
        if (nextValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextValue),
                nextValue,
                "Next execution ID value must be greater than zero.");
        }

        if (isExhausted &&
            nextValue != ulong.MaxValue)
        {
            throw new ArgumentException(
                "An exhausted execution ID allocator must be positioned at ulong.MaxValue.",
                nameof(isExhausted));
        }

        NextValue =
            nextValue;

        IsExhausted =
            isExhausted;
    }

}


