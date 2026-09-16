using Plaquewright.Core.Combat;
using Plaquewright.Core.Tests.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRuntimeOwnershipTests
{
    [Fact]
    public void ForeignGameplayExecution_WithCollidingIds_IsRejectedBeforeDamageIdAllocation()
    {
        var registry =
            CreateRegistry();

        var firstRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var secondRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var firstEntity =
            firstRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var secondEntity =
            secondRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var foreignExecution =
            firstRuntime.CreateExecutionContext(
                firstEntity.Id,
                new SimulationTime(100L));

        var localExecution =
            secondRuntime.CreateExecutionContext(
                secondEntity.Id,
                new SimulationTime(100L));

        // Deliberately prove that the public numeric identity
        // cannot distinguish the two runtimes.
        Assert.Equal(
            foreignExecution.Id,
            localExecution.Id);

        Assert.Equal(
            foreignExecution.SourceEntityId,
            localExecution.SourceEntityId);

        Assert.Throws<InvalidOperationException>(
            () =>
                secondRuntime.CreateDamageExecutionContext(
                    foreignExecution,
                    new SimulationTime(200L)));

        // Failed foreign ownership validation must not consume
        // the deterministic DamageExecutionId.
        var localDamageExecution =
            secondRuntime.CreateDamageExecutionContext(
                localExecution,
                new SimulationTime(200L));

        Assert.Equal(
            new DamageExecutionId(1UL),
            localDamageExecution.Id);
    }

    [Fact]
    public void ForeignDamageExecution_WithCollidingIds_IsRejectedByDamageTargetFactory()
    {
        var registry =
            CreateRegistry();

        var firstRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var secondRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var firstEntity =
            firstRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var secondEntity =
            secondRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var firstGameplay =
            firstRuntime.CreateExecutionContext(
                firstEntity.Id,
                new SimulationTime(100L));

        var secondGameplay =
            secondRuntime.CreateExecutionContext(
                secondEntity.Id,
                new SimulationTime(100L));

        var foreignDamage =
            firstRuntime.CreateDamageExecutionContext(
                firstGameplay,
                new SimulationTime(200L));

        var localDamage =
            secondRuntime.CreateDamageExecutionContext(
                secondGameplay,
                new SimulationTime(200L));

        Assert.Equal(
            foreignDamage.Id,
            localDamage.Id);

        Assert.Equal(
            foreignDamage.SourceEntityId,
            localDamage.SourceEntityId);

        Assert.Throws<InvalidOperationException>(
            () =>
                secondRuntime.CreateDamageTargetContext(
                    foreignDamage,
                    secondEntity.Id));

        var localTarget =
            secondRuntime.CreateDamageTargetContext(
                localDamage,
                secondEntity.Id);

        Assert.Equal(
            localDamage.Id,
            localTarget.DamageExecutionId);

        Assert.Equal(
            secondEntity.Id,
            localTarget.TargetEntityId);
    }

    [Fact]
    public void ForeignHitExecution_WithCollidingIds_IsRejectedWhenCreatingDamageTarget()
    {
        var registry =
            CreateRegistry();

        var firstRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var secondRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var firstEntity =
            firstRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var secondEntity =
            secondRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var firstGameplay =
            firstRuntime.CreateExecutionContext(
                firstEntity.Id,
                new SimulationTime(100L));

        var secondGameplay =
            secondRuntime.CreateExecutionContext(
                secondEntity.Id,
                new SimulationTime(100L));

        var foreignHit =
            firstRuntime.CreateHitExecutionContext(
                firstGameplay,
                firstEntity.Id,
                new SimulationTime(200L));

        var localHit =
            secondRuntime.CreateHitExecutionContext(
                secondGameplay,
                secondEntity.Id,
                new SimulationTime(200L));

        var localDamage =
            secondRuntime.CreateDamageExecutionContext(
                secondGameplay,
                new SimulationTime(200L));

        Assert.Equal(
            foreignHit.Id,
            localHit.Id);

        Assert.Equal(
            foreignHit.TargetEntityId,
            localHit.TargetEntityId);

        Assert.Throws<InvalidOperationException>(
            () =>
                secondRuntime.CreateDamageTargetContext(
                    localDamage,
                    foreignHit));

        var localTarget =
            secondRuntime.CreateDamageTargetContext(
                localDamage,
                localHit);

        Assert.Equal(
            localDamage.Id,
            localTarget.DamageExecutionId);

        Assert.Equal(
            localHit.Id,
            localTarget.RelatedHitExecutionId);
    }
    [Fact]
    public void ForeignDamageTarget_WithCollidingIds_IsRejectedByDamageResolutionFactory()
    {
        var registry =
            CreateRegistry();

        var firstRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var secondRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var firstEntity =
            firstRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var secondEntity =
            secondRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var firstGameplay =
            firstRuntime.CreateExecutionContext(
                firstEntity.Id,
                new SimulationTime(100L));

        var secondGameplay =
            secondRuntime.CreateExecutionContext(
                secondEntity.Id,
                new SimulationTime(100L));

        var firstDamage =
            firstRuntime.CreateDamageExecutionContext(
                firstGameplay,
                new SimulationTime(200L));

        var secondDamage =
            secondRuntime.CreateDamageExecutionContext(
                secondGameplay,
                new SimulationTime(200L));

        var foreignTarget =
            firstRuntime.CreateDamageTargetContext(
                firstDamage,
                firstEntity.Id);

        var localTarget =
            secondRuntime.CreateDamageTargetContext(
                secondDamage,
                secondEntity.Id);

        // Public numeric identity deliberately collides.
        Assert.Equal(
            foreignTarget.DamageExecutionId,
            localTarget.DamageExecutionId);

        Assert.Equal(
            foreignTarget.TargetEntityId,
            localTarget.TargetEntityId);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(10d),
                postMitigationAmount: 10d,
                postTakenScalingAmount: 10d,
                damageTakenAmount: 10d);

        Assert.Throws<InvalidOperationException>(
            () =>
                secondRuntime.CreateDamageResolutionContext(
                    secondDamage,
                    foreignTarget,
                    quantities));

        var localResolution =
            secondRuntime.CreateDamageResolutionContext(
                secondDamage,
                localTarget,
                quantities);

        Assert.Equal(
            secondDamage.Id,
            localResolution.DamageExecutionId);

        Assert.Equal(
            secondEntity.Id,
            localResolution.TargetEntityId);
    }
    [Fact]
    public void UnboundDamageTarget_IsRejectedByRuntimeDamageResolutionFactory()
    {
        var registry =
            CreateRegistry();

        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var entity =
            runtime.CreateEntity(
                Array.Empty<ResourceState>());

        var gameplay =
            runtime.CreateExecutionContext(
                entity.Id,
                new SimulationTime(100L));

        var damageExecution =
            runtime.CreateDamageExecutionContext(
                gameplay,
                new SimulationTime(200L));

        var unboundTarget =
            new DamageTargetContext(
                damageExecution.Id,
                entity.Id,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(10d),
                postMitigationAmount: 10d,
                postTakenScalingAmount: 10d,
                damageTakenAmount: 10d);

        Assert.Throws<InvalidOperationException>(
            () =>
                runtime.CreateDamageResolutionContext(
                    damageExecution,
                    unboundTarget,
                    quantities));
    }

    [Fact]
    public void ForeignGameplayExecution_WithCollidingIds_IsRejectedBeforeHitIdAllocation()
    {
        var registry =
            CreateRegistry();

        var firstRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var secondRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var firstEntity =
            firstRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var secondEntity =
            secondRuntime.CreateEntity(
                Array.Empty<ResourceState>());

        var foreignExecution =
            firstRuntime.CreateExecutionContext(
                firstEntity.Id,
                new SimulationTime(100L));

        var localExecution =
            secondRuntime.CreateExecutionContext(
                secondEntity.Id,
                new SimulationTime(100L));

        Assert.Equal(
            foreignExecution.Id,
            localExecution.Id);

        Assert.Equal(
            foreignExecution.SourceEntityId,
            localExecution.SourceEntityId);

        Assert.Throws<InvalidOperationException>(
            () =>
                secondRuntime.CreateHitExecutionContext(
                    foreignExecution,
                    secondEntity.Id,
                    new SimulationTime(200L)));

        var localHitExecution =
            secondRuntime.CreateHitExecutionContext(
                localExecution,
                secondEntity.Id,
                new SimulationTime(200L));

        Assert.Equal(
            new HitExecutionId(1UL),
            localHitExecution.Id);
    }

    private static CompiledResourceRegistry CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
        ]);
    }
}