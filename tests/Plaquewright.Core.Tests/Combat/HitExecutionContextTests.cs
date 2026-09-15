using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitExecutionContextTests
{
    [Fact]
    public void Constructor_PreservesSemanticIdentity()
    {
        var context =
            new HitExecutionContext(
                new HitExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new EntityId(40UL),
                new SimulationTime(500L));

        Assert.Equal(
            new HitExecutionId(10UL),
            context.Id);

        Assert.Equal(
            new ExecutionId(20UL),
            context.GameplayExecutionId);

        Assert.Equal(
            new EntityId(30UL),
            context.SourceEntityId);

        Assert.Equal(
            new EntityId(40UL),
            context.TargetEntityId);

        Assert.Equal(
            new SimulationTime(500L),
            context.StartedAt);
    }

    [Fact]
    public void Constructor_AllowsSourceToEqualTarget()
    {
        var entityId =
            new EntityId(30UL);

        var context =
            new HitExecutionContext(
                new HitExecutionId(10UL),
                new ExecutionId(20UL),
                entityId,
                entityId,
                new SimulationTime(0L));

        Assert.Equal(
            entityId,
            context.SourceEntityId);

        Assert.Equal(
            entityId,
            context.TargetEntityId);
    }

    [Fact]
    public void Constructor_WithInvalidHitExecutionId_Throws()
    {
        HitExecutionId id =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new HitExecutionContext(
                    id,
                    new ExecutionId(20UL),
                    new EntityId(30UL),
                    new EntityId(40UL),
                    new SimulationTime(0L)));
    }

    [Fact]
    public void Constructor_WithInvalidGameplayExecutionId_Throws()
    {
        ExecutionId gameplayExecutionId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new HitExecutionContext(
                    new HitExecutionId(10UL),
                    gameplayExecutionId,
                    new EntityId(30UL),
                    new EntityId(40UL),
                    new SimulationTime(0L)));
    }

    [Fact]
    public void Constructor_WithInvalidSourceEntityId_Throws()
    {
        EntityId sourceEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new HitExecutionContext(
                    new HitExecutionId(10UL),
                    new ExecutionId(20UL),
                    sourceEntityId,
                    new EntityId(40UL),
                    new SimulationTime(0L)));
    }

    [Fact]
    public void Constructor_WithInvalidTargetEntityId_Throws()
    {
        EntityId targetEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new HitExecutionContext(
                    new HitExecutionId(10UL),
                    new ExecutionId(20UL),
                    new EntityId(30UL),
                    targetEntityId,
                    new SimulationTime(0L)));
    }
}