namespace Idler.Core.Simulation;

using Idler.Core.Entities;

public sealed class EntityIdAllocator
{
    private ulong _nextValue;
    private bool _isExhausted;

    public EntityIdAllocator()
        : this(1UL)
    {
    }

    internal EntityIdAllocator(
        ulong nextValue)
    {
        if (nextValue == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextValue),
                nextValue,
                "Next entity ID value must be greater than zero.");
        }

        _nextValue = nextValue;
    }

    public EntityId Allocate()
    {
        if (_isExhausted)
        {
            throw new OverflowException(
                "Entity ID space has been exhausted.");
        }

        var id =
            new EntityId(
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