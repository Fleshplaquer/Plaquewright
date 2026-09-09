using Idler.Core.Simulation;
using Idler.Core.Entities;

namespace Idler.Core.Tests.Simulation;

public sealed class EntityIdAllocatorTests
{
    [Fact]
    public void FirstAllocation_ReturnsOne()
    {
        var allocator =
            new EntityIdAllocator();

        var id =
            allocator.Allocate();

        Assert.Equal(
            new EntityId(1UL),
            id);
    }

    [Fact]
    public void ConsecutiveAllocations_AreMonotonic()
    {
        var allocator =
            new EntityIdAllocator();

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        var third =
            allocator.Allocate();

        Assert.Equal(
            new EntityId(1UL),
            first);

        Assert.Equal(
            new EntityId(2UL),
            second);

        Assert.Equal(
            new EntityId(3UL),
            third);
    }

    [Fact]
    public void SeparateAllocators_WithSameInitialState_ProduceSameSequence()
    {
        var first =
            new EntityIdAllocator();

        var second =
            new EntityIdAllocator();

        for (var index = 0;
             index < 100;
             index++)
        {
            Assert.Equal(
                first.Allocate(),
                second.Allocate());
        }
    }

    [Fact]
    public void InternalConstructor_StartsAtConfiguredValue()
    {
        var allocator =
            new EntityIdAllocator(
                42UL);

        Assert.Equal(
            new EntityId(42UL),
            allocator.Allocate());

        Assert.Equal(
            new EntityId(43UL),
            allocator.Allocate());
    }

    [Fact]
    public void InternalConstructor_WithZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new EntityIdAllocator(
                    0UL));
    }

    [Fact]
    public void MaximumValue_CanBeAllocatedExactlyOnce()
    {
        var allocator =
            new EntityIdAllocator(
                ulong.MaxValue);

        var id =
            allocator.Allocate();

        Assert.Equal(
            new EntityId(
                ulong.MaxValue),
            id);
    }

    [Fact]
    public void AllocationAfterMaximumValue_Throws()
    {
        var allocator =
            new EntityIdAllocator(
                ulong.MaxValue);

        _ = allocator.Allocate();

        Assert.Throws<OverflowException>(
            () =>
                allocator.Allocate());
    }

    [Fact]
    public void NearMaximumSequence_RemainsCorrect()
    {
        var allocator =
            new EntityIdAllocator(
                ulong.MaxValue - 1UL);

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        Assert.Equal(
            new EntityId(
                ulong.MaxValue - 1UL),
            first);

        Assert.Equal(
            new EntityId(
                ulong.MaxValue),
            second);

        Assert.Throws<OverflowException>(
            () =>
                allocator.Allocate());
    }
}