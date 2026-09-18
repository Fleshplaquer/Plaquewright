namespace Plaquewright.Core.Simulation;

using Plaquewright.Core.Entities;

public sealed class EntityIdAllocator
{
    private ulong _nextValue;
    private bool _isExhausted;

    public EntityIdAllocator()
        : this(1UL)
    {
    }
    private EntityIdAllocator(
    EntityIdAllocatorSnapshot snapshot)
    {
        _nextValue =
            snapshot.NextValue;

        _isExhausted =
            snapshot.IsExhausted;
    }

    internal EntityIdAllocatorSnapshot CaptureSnapshot()
    {
        return new EntityIdAllocatorSnapshot(
            _nextValue,
            _isExhausted);
    }

    internal static EntityIdAllocator Restore(
        EntityIdAllocatorSnapshot snapshot)
    {
        return new EntityIdAllocator(
            snapshot);
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