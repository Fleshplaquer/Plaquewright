using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Simulation;

internal sealed class HitExecutionIdAllocator
{
    private ulong _nextValue;

    public HitExecutionIdAllocator()
        : this(
            startValue: 1UL)
    {
    }

    internal HitExecutionIdAllocator(
        ulong startValue)
    {
        if (startValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startValue),
                startValue,
                "Hit execution ID allocation must start above zero.");
        }

        _nextValue =
            startValue;
    }

    private HitExecutionIdAllocator(
    HitExecutionIdAllocatorSnapshot snapshot)
    {
        _nextValue =
            snapshot.NextValue;
    }

    internal HitExecutionIdAllocatorSnapshot CaptureSnapshot()
    {
        return new HitExecutionIdAllocatorSnapshot(
            _nextValue);
    }

    internal static HitExecutionIdAllocator Restore(
        HitExecutionIdAllocatorSnapshot snapshot)
    {
        return new HitExecutionIdAllocator(
            snapshot);
    }

    public HitExecutionId Allocate()
    {
        if (_nextValue == 0UL)
        {
            throw new OverflowException(
                "Hit execution ID space has been exhausted.");
        }

        var allocated =
            new HitExecutionId(
                _nextValue);

        unchecked
        {
            _nextValue++;
        }

        return allocated;
    }
}