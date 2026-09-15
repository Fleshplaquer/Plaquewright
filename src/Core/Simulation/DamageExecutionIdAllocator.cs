using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Simulation;

internal sealed class DamageExecutionIdAllocator
{
    private ulong _nextValue;

    public DamageExecutionIdAllocator()
        : this(
            startValue: 1UL)
    {
    }

    internal DamageExecutionIdAllocator(
        ulong startValue)
    {
        if (startValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startValue),
                startValue,
                "Damage execution ID allocation must start above zero.");
        }

        _nextValue =
            startValue;
    }

    public DamageExecutionId Allocate()
    {
        if (_nextValue == 0UL)
        {
            throw new OverflowException(
                "Damage execution ID space has been exhausted.");
        }

        var allocated =
            new DamageExecutionId(
                _nextValue);

        unchecked
        {
            _nextValue++;
        }

        return allocated;
    }
}