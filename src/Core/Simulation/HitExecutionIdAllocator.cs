using Idler.Core.Combat;

namespace Idler.Core.Simulation;

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