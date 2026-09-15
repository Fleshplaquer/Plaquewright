namespace Plaquewright.Core.Simulation;

public sealed class ExecutionIdAllocator
{
    private ulong _nextValue;
    private bool _isExhausted;

    public ExecutionIdAllocator()
        : this(1UL)
    {
    }

    internal ExecutionIdAllocator(
        ulong nextValue)
    {
        if (nextValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextValue),
                nextValue,
                "Next execution ID value must be greater than zero.");
        }

        _nextValue = nextValue;
    }

    public ExecutionId Allocate()
    {
        if (_isExhausted)
        {
            throw new OverflowException(
                "Execution ID space has been exhausted.");
        }

        var id =
            new ExecutionId(
                _nextValue);

        if (_nextValue ==
            ulong.MaxValue)
        {
            _isExhausted = true;
        }
        else
        {
            _nextValue++;
        }

        return id;
    }
}