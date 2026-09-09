using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationRuntimeStateTests
{
    [Fact]
    public void Constructor_PreservesSeedAndRegistry()
    {
        var registry =
            CreateRegistry();

        var seed =
            new SimulationSeed(
                123UL);

        var runtime =
            new SimulationRuntimeState(
                seed,
                registry);

        Assert.Equal(
            seed,
            runtime.RootSeed);

        Assert.Same(
            registry,
            runtime.ResourceRegistry);

        Assert.Same(
            registry,
            runtime.Entities.ResourceRegistry);
    }

    [Fact]
    public void NewRuntime_HasNoEntities()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        Assert.Equal(
            0,
            runtime.Entities.Count);
    }

    [Fact]
    public void CreateEntity_AllocatesMonotonicEntityIds()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        var first =
            runtime.CreateEntity(
                []);

        var second =
            runtime.CreateEntity(
                []);

        var third =
            runtime.CreateEntity(
                []);

        Assert.Equal(
            new EntityId(1UL),
            first.Id);

        Assert.Equal(
            new EntityId(2UL),
            second.Id);

        Assert.Equal(
            new EntityId(3UL),
            third.Id);
    }

    [Fact]
    public void CreateEntity_AddsEntityToRuntimeSet()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        var entity =
            runtime.CreateEntity(
                []);

        Assert.Equal(
            1,
            runtime.Entities.Count);

        Assert.Same(
            entity,
            runtime.Entities.Get(
                entity.Id));
    }

    [Fact]
    public void CreateEntity_UsesRuntimeResourceRegistry()
    {
        var registry =
            CreateRegistry();

        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                registry);

        var entity =
            runtime.CreateEntity(
                []);

        Assert.Same(
            registry,
            entity.ResourceRegistry);
    }

    [Fact]
    public void CreateExecutionContext_AllocatesMonotonicExecutionIds()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        var source =
            runtime.CreateEntity(
                []);

        var first =
            runtime.CreateExecutionContext(
                source.Id,
                SimulationTime.Zero);

        var second =
            runtime.CreateExecutionContext(
                source.Id,
                SimulationTime.Zero);

        Assert.Equal(
            new ExecutionId(1UL),
            first.Id);

        Assert.Equal(
            new ExecutionId(2UL),
            second.Id);
    }

    [Fact]
    public void CreateExecutionContext_PreservesSourceAndTime()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        var source =
            runtime.CreateEntity(
                []);

        var startedAt =
            new SimulationTime(
                123);

        var context =
            runtime.CreateExecutionContext(
                source.Id,
                startedAt);

        Assert.Equal(
            source.Id,
            context.SourceEntityId);

        Assert.Equal(
            startedAt,
            context.StartedAt);
    }

    [Fact]
    public void CreateExecutionContext_ForUnknownEntity_Throws()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        Assert.Throws<KeyNotFoundException>(
            () =>
                runtime.CreateExecutionContext(
                    new EntityId(42UL),
                    SimulationTime.Zero));
    }

    [Fact]
    public void CreateExecutionContext_WithInvalidEntityId_Throws()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        EntityId source = default;

        Assert.Throws<ArgumentException>(
            () =>
                runtime.CreateExecutionContext(
                    source,
                    SimulationTime.Zero));
    }

    [Fact]
    public void EqualRuntimeSetup_ProducesEqualIdentitySequences()
    {
        var first =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                CreateRegistry());

        var second =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                CreateRegistry());

        var firstEntity =
            first.CreateEntity(
                []);

        var secondEntity =
            second.CreateEntity(
                []);

        var firstExecution =
            first.CreateExecutionContext(
                firstEntity.Id,
                SimulationTime.Zero);

        var secondExecution =
            second.CreateExecutionContext(
                secondEntity.Id,
                SimulationTime.Zero);

        Assert.Equal(
            firstEntity.Id,
            secondEntity.Id);

        Assert.Equal(
            firstExecution.Id,
            secondExecution.Id);
    }

    [Fact]
    public void Constructor_WithNullRegistry_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new SimulationRuntimeState(
                    new SimulationSeed(1UL),
                    null!));
    }

    [Fact]
    public void CreateEntity_WithNullResources_Throws()
    {
        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(1UL),
                CreateRegistry());

        Assert.Throws<ArgumentNullException>(
            () =>
                runtime.CreateEntity(
                    null!));
    }

    private static CompiledResourceRegistry
        CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource)
        ]);
    }
}