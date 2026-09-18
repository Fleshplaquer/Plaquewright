using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRuntimeStateSnapshotTests
{
    [Fact]
    public void SnapshotRestore_PreservesEntityStateAndIndependentContinuation()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var originalEntity =
            original.CreateEntity(
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var originalLife =
            originalEntity.Resources.Get(
                lifeId);

        originalLife.SetValues(
            current: 75d,
            maximum: 100d);

        var snapshot =
            original.CaptureSnapshot();

        //
        // Changes after capture must not leak back
        // into the snapshot.
        //
        originalLife.SetValues(
            current: 50d,
            maximum: 100d);

        var originalNextEntity =
            original.CreateEntity(
                []);

        var restored =
            SimulationRuntimeState.Restore(
                snapshot,
                registry);

        var restoredEntity =
            restored.Entities.Get(
                originalEntity.Id);

        var restoredLife =
            restoredEntity.Resources.Get(
                lifeId);

        Assert.Equal(
            original.RootSeed,
            restored.RootSeed);

        Assert.Same(
            registry,
            restored.ResourceRegistry);

        Assert.Equal(
            75d,
            restoredLife.Current);

        Assert.Equal(
            100d,
            restoredLife.Maximum);

        Assert.Equal(
            1UL,
            restoredLife.Revision);

        Assert.NotSame(
            originalEntity,
            restoredEntity);

        Assert.NotSame(
            originalLife,
            restoredLife);

        Assert.Equal(
            50d,
            originalLife.Current);

        Assert.Equal(
            2UL,
            originalLife.Revision);

        var restoredNextEntity =
            restored.CreateEntity(
                []);

        Assert.Equal(
            originalNextEntity.Id,
            restoredNextEntity.Id);
    }
    [Fact]
    public void SnapshotRestore_PreservesExecutionPositionsAndRebindsRuntimeOwnership()
    {
        var registry =
            CreateRegistry();

        var source =
            new SimulationRuntimeState(
                new SimulationSeed(456UL),
                registry);

        var sourceEntity =
            source.CreateEntity(
                []);

        var targetEntity =
            source.CreateEntity(
                []);

        var firstExecution =
            source.CreateExecutionContext(
                sourceEntity.Id,
                new SimulationTime(10L));

        var firstHit =
            source.CreateHitExecutionContext(
                firstExecution,
                targetEntity.Id,
                new SimulationTime(10L));

        var firstDamage =
            source.CreateDamageExecutionContext(
                firstExecution,
                new SimulationTime(10L));

        Assert.Equal(
            1UL,
            firstExecution.Id.Value);

        Assert.Equal(
            1UL,
            firstHit.Id.Value);

        Assert.Equal(
            1UL,
            firstDamage.Id.Value);

        var snapshot =
            source.CaptureSnapshot();

        var restored =
            SimulationRuntimeState.Restore(
                snapshot,
                registry);

        //
        // Runtime-bound objects from the old runtime
        // must not become valid in the restored one.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                restored.CreateDamageExecutionContext(
                    firstExecution,
                    new SimulationTime(20L)));

        var sourceSecondExecution =
            source.CreateExecutionContext(
                sourceEntity.Id,
                new SimulationTime(20L));

        var restoredSecondExecution =
            restored.CreateExecutionContext(
                sourceEntity.Id,
                new SimulationTime(20L));

        Assert.Equal(
            sourceSecondExecution.Id,
            restoredSecondExecution.Id);

        Assert.Equal(
            new ExecutionId(2UL),
            restoredSecondExecution.Id);

        var sourceSecondHit =
            source.CreateHitExecutionContext(
                sourceSecondExecution,
                targetEntity.Id,
                new SimulationTime(20L));

        var restoredSecondHit =
            restored.CreateHitExecutionContext(
                restoredSecondExecution,
                targetEntity.Id,
                new SimulationTime(20L));

        Assert.Equal(
            sourceSecondHit.Id,
            restoredSecondHit.Id);

        Assert.Equal(
            new HitExecutionId(2UL),
            restoredSecondHit.Id);

        var sourceSecondDamage =
            source.CreateDamageExecutionContext(
                sourceSecondExecution,
                new SimulationTime(20L));

        var restoredSecondDamage =
            restored.CreateDamageExecutionContext(
                restoredSecondExecution,
                new SimulationTime(20L));

        Assert.Equal(
            sourceSecondDamage.Id,
            restoredSecondDamage.Id);

        Assert.Equal(
            new DamageExecutionId(2UL),
            restoredSecondDamage.Id);
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
                ResourceRole.DefeatRelevant)
        ]);
    }
}