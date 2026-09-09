using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationRuntimeHitExecutionTests
{
    [Fact]
    public void CreateHitExecutionContext_LinksGameplayExecutionAndTarget()
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

        var hit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(500L));

        Assert.Equal(
            new HitExecutionId(1UL),
            hit.Id);

        Assert.Equal(
            gameplayExecution.Id,
            hit.GameplayExecutionId);

        Assert.Equal(
            source.Id,
            hit.SourceEntityId);

        Assert.Equal(
            target.Id,
            hit.TargetEntityId);

        Assert.Equal(
            new SimulationTime(500L),
            hit.StartedAt);
    }

    [Fact]
    public void SameGameplayExecution_CanCreateMultipleDistinctHits()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var firstTarget =
            runtime.CreateEntity(
                []);

        var secondTarget =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var firstHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                firstTarget.Id,
                new SimulationTime(200L));

        var secondHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                secondTarget.Id,
                new SimulationTime(300L));

        Assert.Equal(
            new HitExecutionId(1UL),
            firstHit.Id);

        Assert.Equal(
            new HitExecutionId(2UL),
            secondHit.Id);

        Assert.Equal(
            gameplayExecution.Id,
            firstHit.GameplayExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            secondHit.GameplayExecutionId);

        Assert.NotEqual(
            firstHit.TargetEntityId,
            secondHit.TargetEntityId);
    }

    [Fact]
    public void MissingTarget_IsRejectedBeforeHitIdAllocation()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var validTarget =
            runtime.CreateEntity(
                []);

        var gameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        Assert.Throws<KeyNotFoundException>(
            () =>
                runtime.CreateHitExecutionContext(
                    gameplayExecution,
                    new EntityId(999UL),
                    new SimulationTime(200L)));

        var validHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                validTarget.Id,
                new SimulationTime(200L));

        Assert.Equal(
            new HitExecutionId(1UL),
            validHit.Id);
    }

    [Fact]
    public void InvalidTarget_IsRejectedBeforeHitIdAllocation()
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

        EntityId invalidTarget =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                runtime.CreateHitExecutionContext(
                    gameplayExecution,
                    invalidTarget,
                    new SimulationTime(200L)));

        var validHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(200L));

        Assert.Equal(
            new HitExecutionId(1UL),
            validHit.Id);
    }

    [Fact]
    public void HitTime_IsIndependentFromGameplayExecutionStartTime()
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

        var hit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(5_000L));

        Assert.Equal(
            new SimulationTime(100L),
            gameplayExecution.StartedAt);

        Assert.Equal(
            new SimulationTime(5_000L),
            hit.StartedAt);
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