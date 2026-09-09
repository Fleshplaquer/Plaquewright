using Idler.Core.Entities;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class GameplayExecutionContextTests
{
    [Fact]
    public void Constructor_PreservesExecutionIdentitySourceAndTime()
    {
        var id =
            new ExecutionId(42UL);

        var source =
            new EntityId(7UL);

        var startedAt =
            new SimulationTime(123);

        var context =
            new GameplayExecutionContext(
                id,
                source,
                startedAt,
                new SimulationSeed(123456789UL));

        Assert.Equal(
            id,
            context.Id);

        Assert.Equal(
            source,
            context.SourceEntityId);

        Assert.Equal(
            startedAt,
            context.StartedAt);
    }

    [Fact]
    public void Constructor_WithInvalidExecutionId_Throws()
    {
        ExecutionId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new GameplayExecutionContext(
                    id,
                    new EntityId(1UL),
                    SimulationTime.Zero,
                new SimulationSeed(123456789UL)));
    }

    [Fact]
    public void Constructor_WithInvalidSourceEntityId_Throws()
    {
        EntityId source = default;

        Assert.Throws<ArgumentException>(
            () =>
                new GameplayExecutionContext(
                    new ExecutionId(1UL),
                    source,
                    SimulationTime.Zero,
                new SimulationSeed(123456789UL)));
    }

    [Fact]
    public void ZeroStartTime_IsValid()
    {
        var context =
            new GameplayExecutionContext(
                new ExecutionId(1UL),
                new EntityId(1UL),
                SimulationTime.Zero,
                new SimulationSeed(123456789UL));

        Assert.Equal(
            SimulationTime.Zero,
            context.StartedAt);
    }
    [Fact]
    public void CreateRngStream_WithSameContextAndDomain_IsReproducible()
    {
        var context =
            new GameplayExecutionContext(
                new ExecutionId(42UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                new SimulationSeed(123456789UL));

        var first =
            context.CreateRngStream(
                RngDomainKey.Parse(
                    "combat.crit"));

        var second =
            context.CreateRngStream(
                RngDomainKey.Parse(
                    "combat.crit"));

        for (var index = 0;
             index < 100;
             index++)
        {
            Assert.Equal(
                first.NextUInt64(),
                second.NextUInt64());
        }
    }
    [Fact]
    public void CreateRngStream_WithDifferentDomains_ProducesDifferentStreams()
    {
        var context =
            new GameplayExecutionContext(
                new ExecutionId(42UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                new SimulationSeed(123456789UL));

        var crit =
            context.CreateRngStream(
                RngDomainKey.Parse(
                    "combat.crit"));

        var damage =
            context.CreateRngStream(
                RngDomainKey.Parse(
                    "combat.damage"));

        Assert.NotEqual(
            crit.NextUInt64(),
            damage.NextUInt64());
    }
    [Fact]
    public void DifferentExecutionIds_ProduceDifferentStreams()
    {
        var seed =
            new SimulationSeed(
                123456789UL);

        var firstContext =
            new GameplayExecutionContext(
                new ExecutionId(1UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                seed);

        var secondContext =
            new GameplayExecutionContext(
                new ExecutionId(2UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                seed);

        var domain =
            RngDomainKey.Parse(
                "combat.crit");

        var first =
            firstContext.CreateRngStream(
                domain);

        var second =
            secondContext.CreateRngStream(
                domain);

        Assert.NotEqual(
            first.NextUInt64(),
            second.NextUInt64());
    }
    [Fact]
    public void DifferentRootSeeds_ProduceDifferentStreams()
    {
        var firstContext =
            new GameplayExecutionContext(
                new ExecutionId(42UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                new SimulationSeed(1UL));

        var secondContext =
            new GameplayExecutionContext(
                new ExecutionId(42UL),
                new EntityId(7UL),
                SimulationTime.Zero,
                new SimulationSeed(2UL));

        var domain =
            RngDomainKey.Parse(
                "combat.crit");

        var first =
            firstContext.CreateRngStream(
                domain);

        var second =
            secondContext.CreateRngStream(
                domain);

        Assert.NotEqual(
            first.NextUInt64(),
            second.NextUInt64());
    }
}