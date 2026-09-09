using Idler.Core.Combat;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class HitExecutionIdAllocatorTests
{
    [Fact]
    public void Allocate_StartsAtOne()
    {
        var allocator =
            new HitExecutionIdAllocator();

        var id =
            allocator.Allocate();

        Assert.Equal(
            new HitExecutionId(1UL),
            id);
    }

    [Fact]
    public void Allocate_IsMonotonic()
    {
        var allocator =
            new HitExecutionIdAllocator();

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        var third =
            allocator.Allocate();

        Assert.Equal(
            new HitExecutionId(1UL),
            first);

        Assert.Equal(
            new HitExecutionId(2UL),
            second);

        Assert.Equal(
            new HitExecutionId(3UL),
            third);
    }

    [Fact]
    public void Constructor_WithExplicitStart_UsesThatValue()
    {
        var allocator =
            new HitExecutionIdAllocator(
                startValue: 42UL);

        Assert.Equal(
            new HitExecutionId(42UL),
            allocator.Allocate());

        Assert.Equal(
            new HitExecutionId(43UL),
            allocator.Allocate());
    }

    [Fact]
    public void Constructor_WithZeroStart_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitExecutionIdAllocator(
                    startValue: 0UL));
    }

    [Fact]
    public void MaximumValue_IsAllocatedOnceThenExhausted()
    {
        var allocator =
            new HitExecutionIdAllocator(
                startValue: ulong.MaxValue);

        var maximum =
            allocator.Allocate();

        Assert.Equal(
            new HitExecutionId(
                ulong.MaxValue),
            maximum);

        Assert.Throws<OverflowException>(
            () =>
                allocator.Allocate());
    }
}