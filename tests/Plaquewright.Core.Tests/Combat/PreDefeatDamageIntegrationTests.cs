using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class PreDefeatDamageIntegrationTests
{
    [Fact]
    public void LethalDamage_CannotCommitBeforePreDefeatPhase()
    {
        var setup =
            CreateSetup();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d);

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var routingResult =
            DamageResourceTransactionStager.Stage(
                draft,
                lossPlan);

        Assert.Equal(
            100d,
            routingResult.Result.ActualLoss);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        // Still invisible.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        // Lethal projection was not allowed to bypass
        // the pre-defeat phase.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        // Projection remains intact for intervention.
        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);
    }

    [Fact]
    public void LethalDamage_WithRecoveryIntervention_CommitsRescuedStateAtomically()
    {
        var setup =
            CreateSetup();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d);

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        DamageResourceTransactionStager.Stage(
            draft,
            lossPlan);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        var intervention =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.True(
            intervention.WasDefeatResolved);

        Assert.Equal(
            25d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        // Still no visible mutation.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            phaseResult.Outcome);

        var ledger =
            new ResourceOperationLedger();

        var commitResult =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented,
            commitResult.Outcome);

        Assert.True(
            commitResult.DefeatWasPrevented);

        Assert.False(
            commitResult.DefeatWasAccepted);

        // One final visible state mutation.
        Assert.Equal(
            25d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        // But causal gross operations remain distinct.
        Assert.Equal(
            2,
            ledger.Count);

        var damageEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            setup.Entity.Id,
            damageEntry.TargetEntityId);

        Assert.Equal(
            setup.LifeId,
            damageEntry.ResourceId);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            damageEntry.Provenance.Cause);

        Assert.Equal(
            100d,
            damageEntry.Result.RequestedLoss);

        Assert.Equal(
            100d,
            damageEntry.Result.ActualLoss);

        var recoveryEntry =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            setup.Entity.Id,
            recoveryEntry.TargetEntityId);

        Assert.Equal(
            setup.LifeId,
            recoveryEntry.ResourceId);

        Assert.Equal(
            ResourceOperationCause.Recovery,
            recoveryEntry.Provenance.Cause);

        Assert.Equal(
            25d,
            recoveryEntry.Result.RequestedRecovery);

        Assert.Equal(
            25d,
            recoveryEntry.Result.ActualRecovery);
    }

    [Fact]
    public void LethalDamage_WithUnresolvedPreDefeat_CommitsAcceptedDefeat()
    {
        var setup =
            CreateSetup();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d);

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var preview =
            DamageResourceTransactionStager.Stage(
                draft,
                lossPlan);

        Assert.Equal(
            100d,
            preview.Result.ActualLoss);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Unresolved,
            phaseResult.Outcome);

        // Still invisible until the explicit defeat-aware commit.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        var ledger =
            new ResourceOperationLedger();

        var commitResult =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted,
            commitResult.Outcome);

        Assert.True(
            commitResult.DefeatWasAccepted);

        Assert.False(
            commitResult.DefeatWasPrevented);

        Assert.True(
            commitResult.FinalEvaluation.IsProjectedDefeated);

        Assert.Equal(
            0d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        var damageEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            damageEntry.Provenance.Cause);

        Assert.Equal(
            100d,
            damageEntry.Result.ActualLoss);
    }

    [Fact]
    public void NonLethalDamage_DoesNotEnterPreDefeatPhase()
    {
        var setup =
            CreateSetup();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 40d);

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        DamageResourceTransactionStager.Stage(
            draft,
            lossPlan);

        Assert.Equal(
            60d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.Null(
            context);

        var ledger =
            new ResourceOperationLedger();

        var commitResult =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition,
            commitResult.Outcome);

        Assert.Equal(
            60d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        Assert.Single(
            ledger.Entries);
    }

    [Fact]
    public void DamageUnitsAndFinalResourceStateRemainSeparateThroughPreDefeatFlow()
    {
        var setup =
            CreateSetup();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 200d);

        Assert.Equal(
            200d,
            resolution.Quantities.Taken.Amount);

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        // Explicit conversion:
        // 200 damage units become 100 requested Life units.
        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var preview =
            DamageResourceTransactionStager.Stage(
                draft,
                lossPlan);

        Assert.Equal(
            200d,
            lossPlan.DamageTaken.Amount);

        Assert.Equal(
            100d,
            preview.Result.RequestedLoss);

        Assert.Equal(
            100d,
            preview.Result.ActualLoss);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        PreDefeatRecoveryIntervention.Apply(
            context,
            setup.LifeId,
            recoveryAmount: 10d);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            phaseResult.Outcome);

        var ledger =
            new ResourceOperationLedger();

        var commitResult =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented,
            commitResult.Outcome);

        Assert.Equal(
            10d,
            setup.LifeTarget.State.Current);

        // No accidental equality contract exists:
        // DamageTaken was 200 damage units.
        // Requested Life loss was 100 resource units.
        // Final Life is 10 after the intervention.
        Assert.Equal(
            200d,
            resolution.Quantities.Taken.Amount);

        Assert.Equal(
            100d,
            lossPlan.RequestedResourceLoss);

        Assert.Equal(
            10d,
            setup.LifeTarget.State.Current);
    }

    private static DamageResolutionContext CreateDamageResolution(
        EntityId targetEntityId,
        double damageTakenAmount)
    {
        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(1UL),
                new EntityId(1UL),
                new SimulationTime(100L));

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                targetEntityId,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(
                    damageTakenAmount),
                postMitigationAmount:
                    damageTakenAmount,
                postTakenScalingAmount:
                    damageTakenAmount,
                damageTakenAmount:
                    damageTakenAmount);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

    private static TestSetup CreateSetup()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            lifeId,
            entity,
            new ResourceStateTarget(
                entity,
                lifeId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget);
}