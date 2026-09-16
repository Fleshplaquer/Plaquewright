using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionCausalExecutionTests
{
    [Fact]
    public void RoutingState_PreservesGameplayExecutionThroughApplyAndFinalize()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var runtimeIdentity =
            new SimulationRuntimeIdentity();

        var gameplayExecutionId =
            new ExecutionId(17UL);

        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(23UL),
                gameplayExecutionId,
                new EntityId(1UL),
                new SimulationTime(100L),
                runtimeIdentity);

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                new EntityId(2UL),
                relatedHitExecutionId: null,
                runtimeIdentity: runtimeIdentity);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                damageExecution,
                damageTarget);

        Assert.True(
            state.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            state.GameplayExecutionId.GetValueOrDefault());

        var assignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 40d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 10d)));

        Assert.True(
            state.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            state.GameplayExecutionId.GetValueOrDefault());

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.True(
            finalized.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            finalized.GameplayExecutionId.GetValueOrDefault());
    }

    [Fact]
    public void Start_WithDifferentDamageExecutionIds_IsRejected()
    {
        var resolution =
            CreateSingleAssignmentResolution();

        var runtimeIdentity =
            new SimulationRuntimeIdentity();

        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(2UL),
                new EntityId(3UL),
                new SimulationTime(100L),
                runtimeIdentity);

        var damageTarget =
            new DamageTargetContext(
                new DamageExecutionId(4UL),
                new EntityId(5UL),
                relatedHitExecutionId: null,
                runtimeIdentity: runtimeIdentity);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionAssignmentRoutingStarter.Start(
                    resolution,
                    damageExecution,
                    damageTarget));
    }

    [Fact]
    public void Start_WithDifferentSimulationRuntimes_IsRejected()
    {
        var resolution =
            CreateSingleAssignmentResolution();

        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(2UL),
                new EntityId(3UL),
                new SimulationTime(100L),
                new SimulationRuntimeIdentity());

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                new EntityId(5UL),
                relatedHitExecutionId: null,
                runtimeIdentity: new SimulationRuntimeIdentity());

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionAssignmentRoutingStarter.Start(
                    resolution,
                    damageExecution,
                    damageTarget));
    }

    [Fact]
    public void SingleResourceRoute_PreservesGameplayExecutionIntoLedger()
    {
        var setup =
            CreateManaSetup(
                currentMana: 50d);

        var resolution =
            CreateSingleAssignmentResolution();

        var gameplayExecutionId =
            new ExecutionId(31UL);

        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(37UL),
                gameplayExecutionId,
                new EntityId(2UL),
                new SimulationTime(100L));

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                setup.ManaTarget.EntityId,
                relatedHitExecutionId: null);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                damageExecution,
                damageTarget);

        var binding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var execution =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                binding);

        Assert.Single(
            draft.Operations);

        Assert.True(
            draft.Operations[0].Provenance.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            draft.Operations[0].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        Assert.True(
            execution.UpdatedState.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            execution.UpdatedState.GameplayExecutionId
                .GetValueOrDefault());

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Single(
            ledger.Entries);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            entry.Provenance.Cause);

        Assert.True(
            entry.Provenance.HasGameplayExecution);

        Assert.Equal(
            gameplayExecutionId,
            entry.Provenance.GameplayExecutionId
                .GetValueOrDefault());
    }

    [Fact]
    public void ContextFreeSingleResourceRoute_RemainsWithoutGameplayExecution()
    {
        var setup =
            CreateManaSetup(
                currentMana: 50d);

        var resolution =
            CreateSingleAssignmentResolution();

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var binding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        ProtectionResourceRouteExecutor.Stage(
            draft,
            state,
            binding);

        Assert.Single(
            draft.Operations);

        Assert.False(
            draft.Operations[0].Provenance.HasGameplayExecution);

        Assert.Null(
            draft.Operations[0].Provenance.GameplayExecutionId);
    }

    private static ProtectionAssignmentResolution CreateSingleAssignmentResolution()
    {
        return ProtectionAssignmentResolver.Resolve(
            damage: 100d,
            [
                new ProtectionAssignmentRequest(
                    requestedFraction: 0.3d)
            ]);
    }

    private static ManaSetup CreateManaSetup(
        double currentMana)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.ProtectionSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        currentMana,
                        maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        return new ManaSetup(
            registry,
            manaTarget);
    }

    private sealed record ManaSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget ManaTarget);
}