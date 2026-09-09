using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationRuntimeDamageResolutionTests
{
    [Fact]
    public void CreateDamageResolutionContext_LinksCompleteResolutionIdentity()
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
                new SimulationTime(200L));

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var damageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                hit);

        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var resolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                quantities);

        Assert.Equal(
            damageExecution.Id,
            resolution.DamageExecutionId);

        Assert.Equal(
            gameplayExecution.Id,
            resolution.GameplayExecutionId);

        Assert.Equal(
            source.Id,
            resolution.SourceEntityId);

        Assert.Equal(
            target.Id,
            resolution.TargetEntityId);

        Assert.Equal(
            hit.Id,
            resolution.RelatedHitExecutionId);

        Assert.True(
            resolution.IsHitBased);

        Assert.Same(
            quantities,
            resolution.Quantities);
    }

    [Fact]
    public void SameDamageExecution_CanProduceMultipleResolutionContexts()
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

        var firstDamageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                firstTarget.Id);

        var secondDamageTarget =
            runtime.CreateDamageTargetContext(
                damageExecution,
                secondTarget.Id);

        var firstQuantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var secondQuantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 50d,
                postTakenScalingAmount: 40d,
                damageTakenAmount: 30d);

        var firstResolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                firstDamageTarget,
                firstQuantities);

        var secondResolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                secondDamageTarget,
                secondQuantities);

        Assert.Equal(
            firstResolution.DamageExecutionId,
            secondResolution.DamageExecutionId);

        Assert.NotEqual(
            firstResolution.TargetEntityId,
            secondResolution.TargetEntityId);

        Assert.Equal(
            60d,
            firstResolution.Quantities.Taken.Amount);

        Assert.Equal(
            30d,
            secondResolution.Quantities.Taken.Amount);
    }

    [Fact]
    public void DifferentDamageExecutionAndTargetContext_AreRejected()
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

        var firstDamageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var secondDamageExecution =
            runtime.CreateDamageExecutionContext(
                gameplayExecution,
                new SimulationTime(200L));

        var damageTarget =
            runtime.CreateDamageTargetContext(
                firstDamageExecution,
                target.Id);

        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        Assert.Throws<InvalidOperationException>(
            () =>
                runtime.CreateDamageResolutionContext(
                    secondDamageExecution,
                    damageTarget,
                    quantities));
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