using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Combat;

public sealed class CombatExecutionIdentityInvariantTests
{
    [Fact]
    public void OneGameplayExecution_CanOwnMultipleHitsAndDamageExecutions()
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
                new SimulationTime(200L));

        var firstDamage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var secondDamage =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(300L));

        Assert.Equal(
            gameplayExecution.Id,
            firstHit.GameplayExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            secondHit.GameplayExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            firstDamage.GameplayExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            secondDamage.GameplayExecutionId);

        Assert.NotEqual(
            firstHit.Id,
            secondHit.Id);

        Assert.NotEqual(
            firstDamage.Id,
            secondDamage.Id);
    }

    [Fact]
    public void OneDamageExecution_CanResolveAgainstMultipleHits()
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

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var firstHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                firstTarget.Id,
                new SimulationTime(200L));

        var secondHit =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                secondTarget.Id,
                new SimulationTime(200L));

        var firstTargetContext =
            runtime.CreateDamageTargetContext(
                damageExecution,
                firstHit);

        var secondTargetContext =
            runtime.CreateDamageTargetContext(
                damageExecution,
                secondHit);

        Assert.Equal(
            damageExecution.Id,
            firstTargetContext.DamageExecutionId);

        Assert.Equal(
            damageExecution.Id,
            secondTargetContext.DamageExecutionId);

        Assert.Equal(
            firstHit.Id,
            firstTargetContext.RelatedHitExecutionId);

        Assert.Equal(
            secondHit.Id,
            secondTargetContext.RelatedHitExecutionId);

        Assert.NotEqual(
            firstTargetContext.TargetEntityId,
            secondTargetContext.TargetEntityId);
    }

    [Fact]
    public void HitIdentity_IsSharedAcrossDamagePathsByNotExistingPerPath()
    {
        var hitId =
            new HitExecutionId(10UL);

        var damageExecutionId =
            new DamageExecutionId(20UL);

        Assert.True(
            hitId.IsValid);

        Assert.True(
            damageExecutionId.IsValid);

        // Intentionally no DamagePathId exists.
        // Internal damage path splitting must therefore
        // remain local to a DamageExecution and cannot
        // accidentally create independent hit-scoped budgets.
    }

    [Fact]
    public void HitAndDamageExecutionIdentitySpaces_AreSemanticallyIndependent()
    {
        var hitId =
            new HitExecutionId(1UL);

        var damageExecutionId =
            new DamageExecutionId(1UL);

        Assert.Equal(
            1UL,
            hitId.Value);

        Assert.Equal(
            1UL,
            damageExecutionId.Value);

        Assert.True(
            hitId.IsValid);

        Assert.True(
            damageExecutionId.IsValid);
    }

    [Fact]
    public void DamageTargetWithoutHit_RemainsValid()
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

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var damageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                target.Id);

        Assert.False(
            damageTarget.IsHitBased);

        Assert.Null(
            damageTarget.RelatedHitExecutionId);

        Assert.Equal(
            target.Id,
            damageTarget.TargetEntityId);
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