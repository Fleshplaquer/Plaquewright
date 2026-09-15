using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageExecutionContextTests
{
    [Fact]
    public void Constructor_PreservesSemanticIdentity()
    {
        var context =
            new DamageExecutionContext(
                new DamageExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new SimulationTime(500L));

        Assert.Equal(
            new DamageExecutionId(10UL),
            context.Id);

        Assert.Equal(
            new ExecutionId(20UL),
            context.GameplayExecutionId);

        Assert.Equal(
            new EntityId(30UL),
            context.SourceEntityId);

        Assert.Equal(
            new SimulationTime(500L),
            context.StartedAt);
    }

    [Fact]
    public void Constructor_WithInvalidDamageExecutionId_Throws()
    {
        DamageExecutionId id =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageExecutionContext(
                    id,
                    new ExecutionId(20UL),
                    new EntityId(30UL),
                    new SimulationTime(0L)));
    }

    [Fact]
    public void Constructor_WithInvalidGameplayExecutionId_Throws()
    {
        ExecutionId gameplayExecutionId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageExecutionContext(
                    new DamageExecutionId(10UL),
                    gameplayExecutionId,
                    new EntityId(30UL),
                    new SimulationTime(0L)));
    }

    [Fact]
    public void Constructor_WithInvalidSourceEntityId_Throws()
    {
        EntityId sourceEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageExecutionContext(
                    new DamageExecutionId(10UL),
                    new ExecutionId(20UL),
                    sourceEntityId,
                    new SimulationTime(0L)));
    }
}