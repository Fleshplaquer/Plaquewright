using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class ExecutionIdAllocatorTests
{
    [Fact]
    public void FirstAllocation_ReturnsOne()
    {
        var allocator =
            new ExecutionIdAllocator();

        var id =
            allocator.Allocate();

        Assert.Equal(
            new ExecutionId(1UL),
            id);
    }

    [Fact]
    public void ConsecutiveAllocations_AreMonotonic()
    {
        var allocator =
            new ExecutionIdAllocator();

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        var third =
            allocator.Allocate();

        Assert.Equal(
            new ExecutionId(1UL),
            first);

        Assert.Equal(
            new ExecutionId(2UL),
            second);

        Assert.Equal(
            new ExecutionId(3UL),
            third);
    }

    [Fact]
    public void SeparateAllocators_WithSameInitialState_ProduceSameSequence()
    {
        var first =
            new ExecutionIdAllocator();

        var second =
            new ExecutionIdAllocator();

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
    public void EntityAndExecutionAllocators_HaveIndependentIdentitySpaces()
    {
        var entityAllocator =
            new EntityIdAllocator();

        var executionAllocator =
            new ExecutionIdAllocator();

        var entityId =
            entityAllocator.Allocate();

        var executionId =
            executionAllocator.Allocate();

        Assert.Equal(
            1UL,
            entityId.Value);

        Assert.Equal(
            1UL,
            executionId.Value);
    }

    [Fact]
    public void InternalConstructor_StartsAtConfiguredValue()
    {
        var allocator =
            new ExecutionIdAllocator(
                42UL);

        Assert.Equal(
            new ExecutionId(42UL),
            allocator.Allocate());

        Assert.Equal(
            new ExecutionId(43UL),
            allocator.Allocate());
    }

    [Fact]
    public void InternalConstructor_WithZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ExecutionIdAllocator(
                    0UL));
    }

    [Fact]
    public void MaximumValue_CanBeAllocatedExactlyOnce()
    {
        var allocator =
            new ExecutionIdAllocator(
                ulong.MaxValue);

        var id =
            allocator.Allocate();

        Assert.Equal(
            new ExecutionId(
                ulong.MaxValue),
            id);
    }

    [Fact]
    public void AllocationAfterMaximumValue_Throws()
    {
        var allocator =
            new ExecutionIdAllocator(
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
            new ExecutionIdAllocator(
                ulong.MaxValue - 1UL);

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        Assert.Equal(
            new ExecutionId(
                ulong.MaxValue - 1UL),
            first);

        Assert.Equal(
            new ExecutionId(
                ulong.MaxValue),
            second);

        Assert.Throws<OverflowException>(
            () =>
                allocator.Allocate());
    }
}