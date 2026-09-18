using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class IdAllocatorSnapshotTests
{
    [Fact]
    public void EntityIdAllocator_RestoreContinuesFromCapturedPosition()
    {
        var original =
            new EntityIdAllocator();

        Assert.Equal(
            1UL,
            original.Allocate().Value);

        Assert.Equal(
            2UL,
            original.Allocate().Value);

        var restored =
            EntityIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Equal(
            3UL,
            original.Allocate().Value);

        Assert.Equal(
            3UL,
            restored.Allocate().Value);

        Assert.Equal(
            4UL,
            restored.Allocate().Value);
    }

    [Fact]
    public void ExecutionIdAllocator_RestoreContinuesFromCapturedPosition()
    {
        var original =
            new ExecutionIdAllocator();

        Assert.Equal(
            1UL,
            original.Allocate().Value);

        Assert.Equal(
            2UL,
            original.Allocate().Value);

        var restored =
            ExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Equal(
            3UL,
            original.Allocate().Value);

        Assert.Equal(
            3UL,
            restored.Allocate().Value);

        Assert.Equal(
            4UL,
            restored.Allocate().Value);
    }

    [Fact]
    public void HitExecutionIdAllocator_RestoreContinuesFromCapturedPosition()
    {
        var original =
            new HitExecutionIdAllocator();

        Assert.Equal(
            1UL,
            original.Allocate().Value);

        Assert.Equal(
            2UL,
            original.Allocate().Value);

        var restored =
            HitExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Equal(
            3UL,
            original.Allocate().Value);

        Assert.Equal(
            3UL,
            restored.Allocate().Value);

        Assert.Equal(
            4UL,
            restored.Allocate().Value);
    }

    [Fact]
    public void DamageExecutionIdAllocator_RestoreContinuesFromCapturedPosition()
    {
        var original =
            new DamageExecutionIdAllocator();

        Assert.Equal(
            1UL,
            original.Allocate().Value);

        Assert.Equal(
            2UL,
            original.Allocate().Value);

        var restored =
            DamageExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Equal(
            3UL,
            original.Allocate().Value);

        Assert.Equal(
            3UL,
            restored.Allocate().Value);

        Assert.Equal(
            4UL,
            restored.Allocate().Value);
    }

    [Fact]
    public void EntityIdAllocator_ExhaustedSnapshotRemainsExhausted()
    {
        var original =
            new EntityIdAllocator(
                ulong.MaxValue);

        Assert.Equal(
            ulong.MaxValue,
            original.Allocate().Value);

        var restored =
            EntityIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Throws<OverflowException>(
            () =>
                restored.Allocate());
    }

    [Fact]
    public void ExecutionIdAllocator_ExhaustedSnapshotRemainsExhausted()
    {
        var original =
            new ExecutionIdAllocator(
                ulong.MaxValue);

        Assert.Equal(
            ulong.MaxValue,
            original.Allocate().Value);

        var restored =
            ExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Throws<OverflowException>(
            () =>
                restored.Allocate());
    }

    [Fact]
    public void HitExecutionIdAllocator_ExhaustedSnapshotRemainsExhausted()
    {
        var original =
            new HitExecutionIdAllocator(
                ulong.MaxValue);

        Assert.Equal(
            ulong.MaxValue,
            original.Allocate().Value);

        var restored =
            HitExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Throws<OverflowException>(
            () =>
                restored.Allocate());
    }

    [Fact]
    public void DamageExecutionIdAllocator_ExhaustedSnapshotRemainsExhausted()
    {
        var original =
            new DamageExecutionIdAllocator(
                ulong.MaxValue);

        Assert.Equal(
            ulong.MaxValue,
            original.Allocate().Value);

        var restored =
            DamageExecutionIdAllocator.Restore(
                original.CaptureSnapshot());

        Assert.Throws<OverflowException>(
            () =>
                restored.Allocate());
    }
}