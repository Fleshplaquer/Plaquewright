using Plaquewright.Core.Combat;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResolutionInvariantTests
{
    [Fact]
    public void SameDamageExecution_AllowsTargetSpecificQuantities()
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

        var firstResolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                firstDamageTarget,
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 80d,
                    postTakenScalingAmount: 70d,
                    damageTakenAmount: 60d));

        var secondResolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                secondDamageTarget,
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 50d,
                    postTakenScalingAmount: 40d,
                    damageTakenAmount: 30d));

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
    public void HitBasedResolution_PreservesExactlyTheRelatedHitIdentity()
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

        var resolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 90d,
                    postTakenScalingAmount: 80d,
                    damageTakenAmount: 70d));

        Assert.True(
            resolution.IsHitBased);

        Assert.Equal(
            hit.Id,
            resolution.RelatedHitExecutionId);

        Assert.Equal(
            hit.TargetEntityId,
            resolution.TargetEntityId);

        Assert.Equal(
            hit.SourceEntityId,
            resolution.SourceEntityId);
    }

    [Fact]
    public void NonHitResolution_DoesNotInventHitIdentity()
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

        var resolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                DamageResolutionQuantities.Create(
                    new IncomingDamage(100d),
                    postMitigationAmount: 100d,
                    postTakenScalingAmount: 100d,
                    damageTakenAmount: 100d));

        Assert.False(
            resolution.IsHitBased);

        Assert.Null(
            resolution.RelatedHitExecutionId);
    }

    [Fact]
    public void DamageResolution_DoesNotMutateTargetResources()
    {
        var registry =
            CreateResourceRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var source =
            runtime.CreateEntity(
                []);

        var target =
            runtime.CreateEntity(
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var targetLife =
            target.Resources.Get(
                lifeId);

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

        var resolution =
            runtime.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                DamageResolutionQuantities.Create(
                    new IncomingDamage(80d),
                    postMitigationAmount: 70d,
                    postTakenScalingAmount: 60d,
                    damageTakenAmount: 50d));

        Assert.Equal(
            50d,
            resolution.Quantities.Taken.Amount);

        Assert.Equal(
            100d,
            targetLife.Current);

        Assert.Equal(
            100d,
            targetLife.Maximum);

        Assert.Equal(
            0UL,
            targetLife.Revision);
    }

    [Fact]
    public void DamageTakenAndActualResourceLoss_RemainSeparateQuantities()
    {
        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                damageTakenAmount: 70d);

        var firstResourceLoss =
            quantities.Taken.AdvanceToActualResourceLoss(
                30d);

        var secondResourceLoss =
            quantities.Taken.AdvanceToActualResourceLoss(
                20d);

        Assert.Equal(
            70d,
            quantities.Taken.Amount);

        Assert.Equal(
            30d,
            firstResourceLoss.Amount);

        Assert.Equal(
            20d,
            secondResourceLoss.Amount);
    }

    private static SimulationRuntimeState CreateRuntime()
    {
        return new SimulationRuntimeState(
            new SimulationSeed(123UL),
            ResourceRegistryCompiler.Compile(
                Array.Empty<ResourceDefinition>()));
    }

    private static CompiledResourceRegistry CreateResourceRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
        ]);
    }
}