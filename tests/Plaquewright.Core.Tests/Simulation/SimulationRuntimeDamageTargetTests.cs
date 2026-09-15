using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRuntimeDamageTargetTests
{
    [Fact]
    public void CreateDamageTargetContext_WithoutHit_CreatesNonHitRelationship()
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

        Assert.Equal(
            damageExecution.Id,
            damageTarget.DamageExecutionId);

        Assert.Equal(
            target.Id,
            damageTarget.TargetEntityId);

        Assert.False(
            damageTarget.IsHitBased);

        Assert.Null(
            damageTarget.RelatedHitExecutionId);
    }

    [Fact]
    public void CreateDamageTargetContext_WithHit_UsesHitTarget()
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

        var hitExecution =
            runtime.CreateHitExecutionContext(
                gameplayExecution,
                target.Id,
                new SimulationTime(200L));

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var damageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                hitExecution);

        Assert.Equal(
            damageExecution.Id,
            damageTarget.DamageExecutionId);

        Assert.Equal(
            target.Id,
            damageTarget.TargetEntityId);

        Assert.True(
            damageTarget.IsHitBased);

        Assert.Equal(
            hitExecution.Id,
            damageTarget.RelatedHitExecutionId);
    }

    [Fact]
    public void SameDamageExecution_CanResolveAgainstMultipleHitTargets()
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

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var firstDamageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                firstHit);

        var secondDamageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                secondHit);

        Assert.Equal(
            damageExecution.Id,
            firstDamageTarget.DamageExecutionId);

        Assert.Equal(
            damageExecution.Id,
            secondDamageTarget.DamageExecutionId);

        Assert.Equal(
            firstTarget.Id,
            firstDamageTarget.TargetEntityId);

        Assert.Equal(
            secondTarget.Id,
            secondDamageTarget.TargetEntityId);

        Assert.Equal(
            firstHit.Id,
            firstDamageTarget.RelatedHitExecutionId);

        Assert.Equal(
            secondHit.Id,
            secondDamageTarget.RelatedHitExecutionId);
    }

    [Fact]
    public void HitFromDifferentGameplayExecution_IsRejected()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var target =
            runtime.CreateEntity(
                []);

        var firstGameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var secondGameplayExecution =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(100L));

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                firstGameplayExecution,
                new SimulationTime(200L));

        var hitExecution =
            runtime.CreateHitExecutionContext(
                secondGameplayExecution,
                target.Id,
                new SimulationTime(200L));

        Assert.Throws<InvalidOperationException>(
            () =>
                runtime.CreateDamageTargetContext(
                    damageExecution,
                    hitExecution));
    }

    [Fact]
    public void HitWithDifferentSource_IsRejected()
    {
        var runtime =
            CreateRuntime();

        var source =
            runtime.CreateEntity(
                []);

        var otherSource =
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

        // Deliberately construct an inconsistent hit while
        // retaining the same gameplay execution identity.
        var inconsistentHit =
            new HitExecutionContext(
                new HitExecutionId(999UL),
                gameplayExecution.Id,
                otherSource.Id,
                target.Id,
                new SimulationTime(200L));

        Assert.Throws<InvalidOperationException>(
            () =>
                runtime.CreateDamageTargetContext(
                    damageExecution,
                    inconsistentHit));
    }

    [Fact]
    public void MissingNonHitTarget_IsRejected()
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

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        Assert.Throws<KeyNotFoundException>(
            () =>
                runtime.CreateDamageTargetContext(
                    damageExecution,
                    new EntityId(999UL)));
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