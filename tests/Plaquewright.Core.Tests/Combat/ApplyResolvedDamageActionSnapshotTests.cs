using Plaquewright.Core.Combat;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ApplyResolvedDamageActionSnapshotTests
{
    [Fact]
    public void SnapshotRestore_RebindsResolutionWithoutReResolvingOrAllocatingIds()
    {
        var registry =
            CreateRegistry();

        var original =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var source =
            original.CreateEntity(
                []);

        var target =
            original.CreateEntity(
                []);

        var startedAt =
            new SimulationTime(
                100L);

        var execution =
            original.CreateExecutionContext(
                source.Id,
                startedAt);

        var hit =
            original.CreateHitExecutionContext(
                execution,
                target.Id,
                startedAt);

        var damageExecution =
            original.CreateDamageExecutionContext(
                execution,
                startedAt);

        var damageTarget =
            original.CreateDamageTargetContext(
                damageExecution,
                hit);

        var quantities =
            new DamageResolutionQuantities(
                new IncomingDamage(
                    100d),
                new PostMitigationDamage(
                    83d),
                new PostTakenScalingDamage(
                    71d),
                new DamageTaken(
                    59d));

        var resolution =
            original.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                quantities);

        var action =
            new ApplyResolvedDamageAction(
                resolution);

        var actionSnapshot =
            ApplyResolvedDamageActionSnapshot.Capture(
                action);

        var runtimeSnapshot =
            original.CaptureSnapshot();

        var restoredRuntime =
            SimulationRuntimeState.Restore(
                runtimeSnapshot,
                registry);

        var restoredAction =
            actionSnapshot.Restore(
                restoredRuntime);

        var restoredResolution =
            restoredAction.Resolution;

        Assert.Equal(
            resolution.DamageExecutionId,
            restoredResolution.DamageExecutionId);

        Assert.Equal(
            resolution.GameplayExecutionId,
            restoredResolution.GameplayExecutionId);

        Assert.Equal(
            source.Id,
            restoredResolution.SourceEntityId);

        Assert.Equal(
            target.Id,
            restoredResolution.TargetEntityId);

        Assert.Equal(
            hit.Id,
            restoredResolution.RelatedHitExecutionId);

        Assert.True(
            restoredResolution.IsHitBased);

        Assert.Equal(
            startedAt,
            restoredResolution.StartedAt);

        //
        // Already-resolved quantities are restored
        // exactly; no Combat rule is rerun.
        //
        Assert.Equal(
            100d,
            restoredResolution.Quantities
                .Incoming.Amount);

        Assert.Equal(
            83d,
            restoredResolution.Quantities
                .PostMitigation.Amount);

        Assert.Equal(
            71d,
            restoredResolution.Quantities
                .PostTakenScaling.Amount);

        Assert.Equal(
            59d,
            restoredResolution.Quantities
                .Taken.Amount);

        Assert.NotSame(
            resolution,
            restoredResolution);

        Assert.NotSame(
            resolution.Execution,
            restoredResolution.Execution);

        Assert.NotSame(
            resolution.Target,
            restoredResolution.Target);

        Assert.NotSame(
            resolution.Quantities,
            restoredResolution.Quantities);

        //
        // Runtime ownership must be rebound.
        //
        Assert.NotSame(
            resolution.Execution.RuntimeIdentity,
            restoredResolution.Execution.RuntimeIdentity);

        Assert.Same(
            restoredResolution.Execution.RuntimeIdentity,
            restoredResolution.Target.RuntimeIdentity);

        Assert.Throws<InvalidOperationException>(
            () =>
                restoredRuntime.CreateDamageResolutionContext(
                    resolution.Execution,
                    resolution.Target,
                    resolution.Quantities));

        //
        // Restoring existing pending work must not
        // consume any deterministic IDs.
        //
        var nextExecution =
            restoredRuntime.CreateExecutionContext(
                source.Id,
                new SimulationTime(200L));

        Assert.Equal(
            new ExecutionId(2UL),
            nextExecution.Id);

        var nextHit =
            restoredRuntime.CreateHitExecutionContext(
                nextExecution,
                target.Id,
                new SimulationTime(200L));

        Assert.Equal(
            new HitExecutionId(2UL),
            nextHit.Id);

        var nextDamage =
            restoredRuntime.CreateDamageExecutionContext(
                nextExecution,
                new SimulationTime(200L));

        Assert.Equal(
            new DamageExecutionId(2UL),
            nextDamage.Id);
    }

    [Fact]
    public void SnapshotRestore_WhenTargetEntityDoesNotExist_IsRejected()
    {
        var registry =
            CreateRegistry();

        var original =
            new SimulationRuntimeState(
                new SimulationSeed(456UL),
                registry);

        var source =
            original.CreateEntity(
                []);

        var target =
            original.CreateEntity(
                []);

        var execution =
            original.CreateExecutionContext(
                source.Id,
                SimulationTime.Zero);

        var damageExecution =
            original.CreateDamageExecutionContext(
                execution,
                SimulationTime.Zero);

        var damageTarget =
            original.CreateDamageTargetContext(
                damageExecution,
                target.Id);

        var resolution =
            original.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                new DamageResolutionQuantities(
                    new IncomingDamage(10d),
                    new PostMitigationDamage(9d),
                    new PostTakenScalingDamage(8d),
                    new DamageTaken(7d)));

        var snapshot =
            ApplyResolvedDamageActionSnapshot.Capture(
                new ApplyResolvedDamageAction(
                    resolution));

        var incompleteRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(456UL),
                registry);

        var restoredSource =
            incompleteRuntime.CreateEntity(
                []);

        Assert.Equal(
            source.Id,
            restoredSource.Id);

        Assert.False(
            incompleteRuntime.Entities.Contains(
                target.Id));

        Assert.Throws<KeyNotFoundException>(
            () =>
                snapshot.Restore(
                    incompleteRuntime));
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