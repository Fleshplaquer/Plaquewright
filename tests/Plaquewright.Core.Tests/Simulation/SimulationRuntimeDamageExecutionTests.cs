using Plaquewright.Core.Combat;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRuntimeDamageExecutionTests
{
    [Fact]
    public void CreateDamageExecutionContext_LinksGameplayExecution()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var damage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(500L));

        Assert.Equal(
            new DamageExecutionId(1UL),
            damage.Id);

        Assert.Equal(
            gameplayExecution.Id,
            damage.GameplayExecutionId);

        Assert.Equal(
            source.Id,
            damage.SourceEntityId);

        Assert.Equal(
            new SimulationTime(500L),
            damage.StartedAt);
    }

    [Fact]
    public void SameGameplayExecution_CanCreateMultipleDamageExecutions()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var first =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var second =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(300L));

        Assert.Equal(
            new DamageExecutionId(1UL),
            first.Id);

        Assert.Equal(
            new DamageExecutionId(2UL),
            second.Id);

        Assert.Equal(
            gameplayExecution.Id,
            first.GameplayExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            second.GameplayExecutionId);
    }

    [Fact]
    public void DamageTime_IsIndependentFromGameplayExecutionStartTime()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var damage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(5_000L));

        Assert.Equal(
            new SimulationTime(100L),
            gameplayExecution.StartedAt);

        Assert.Equal(
            new SimulationTime(5_000L),
            damage.StartedAt);
    }

    [Fact]
    public void DamageAndHitExecutionIdSpaces_RemainIndependent()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var target =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var firstDamage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var firstHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(200L));

        var secondHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(300L));

        var secondDamage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(300L));

        Assert.Equal(
            new DamageExecutionId(1UL),
            firstDamage.Id);

        Assert.Equal(
            new DamageExecutionId(2UL),
            secondDamage.Id);

        Assert.Equal(
            new HitExecutionId(1UL),
            firstHit.Id);

        Assert.Equal(
            new HitExecutionId(2UL),
            secondHit.Id);
    }

    private static SimulationRuntimeState CreateRuntime()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
                Array.Empty<ResourceDefinition>());

        return new SimulationRuntimeState(
            new SimulationSeed(123UL),
            registry);
    }
}