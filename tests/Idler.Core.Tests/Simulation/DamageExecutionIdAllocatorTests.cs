using Idler.Core.Combat;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class DamageExecutionIdAllocatorTests
{
    [Fact]
    public void Allocate_StartsAtOne()
    {
        var allocator =
            new DamageExecutionIdAllocator();

        var id =
            allocator.Allocate();

        Assert.Equal(
            new DamageExecutionId(1UL),
            id);
    }

    [Fact]
    public void Allocate_IsMonotonic()
    {
        var allocator =
            new DamageExecutionIdAllocator();

        var first =
            allocator.Allocate();

        var second =
            allocator.Allocate();

        var third =
            allocator.Allocate();

        Assert.Equal(
            new DamageExecutionId(1UL),
            first);

        Assert.Equal(
            new DamageExecutionId(2UL),
            second);

        Assert.Equal(
            new DamageExecutionId(3UL),
            third);
    }

    [Fact]
    public void Constructor_WithExplicitStart_UsesThatValue()
    {
        var allocator =
            new DamageExecutionIdAllocator(
                startValue: 42UL);

        Assert.Equal(
            new DamageExecutionId(42UL),
            allocator.Allocate());

        Assert.Equal(
            new DamageExecutionId(43UL),
            allocator.Allocate());
    }

    [Fact]
    public void Constructor_WithZeroStart_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new DamageExecutionIdAllocator(
                    startValue: 0UL));
    }

    [Fact]
    public void MaximumValue_IsAllocatedOnceThenExhausted()
    {
        var allocator =
            new DamageExecutionIdAllocator(
                startValue: ulong.MaxValue);

        var maximum =
            allocator.Allocate();

        Assert.Equal(
            new DamageExecutionId(
                ulong.MaxValue),
            maximum);

        Assert.Throws<OverflowException>(
            () =>
                allocator.Allocate());
    }
}