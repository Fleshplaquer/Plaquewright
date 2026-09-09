using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class DefeatAwareResourceTransactionCommitterTests
{
    [Fact]
    public void NoDefeatTransition_CommitsNormally()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 40d);

        var ledger =
            new ResourceOperationLedger();

        var result =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition,
            result.Outcome);

        Assert.False(
            result.DefeatWasPrevented);

        Assert.False(
            result.DefeatWasAccepted);

        Assert.False(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.Null(
            result.PreDefeatPhaseResult);

        Assert.Equal(
            60d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            1,
            ledger.Count);
    }

    [Fact]
    public void NewDefeatTransition_WithoutPreDefeatPhase_IsRejectedBeforeCommit()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 100d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        // Nothing became visible.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        // Projection is still available.
        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);
    }

    [Fact]
    public void UnresolvedPreDefeatPhase_CommitsAndAcceptsDefeat()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 100d);

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

        var ledger =
            new ResourceOperationLedger();

        var result =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted,
            result.Outcome);

        Assert.True(
            result.DefeatWasAccepted);

        Assert.False(
            result.DefeatWasPrevented);

        Assert.True(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.True(
            result.FinalEvaluation.IsNewDefeatTransition);

        Assert.Same(
            phaseResult,
            result.PreDefeatPhaseResult);

        Assert.Equal(
            0d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            entry.Provenance.Cause);

        Assert.Equal(
            100d,
            entry.Result.ActualLoss);
    }

    [Fact]
    public void ResolvedPreDefeatPhase_CommitsRescuedState()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 100d);

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
            recoveryAmount: 25d);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            phaseResult.Outcome);

        var ledger =
            new ResourceOperationLedger();

        var result =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented,
            result.Outcome);

        Assert.True(
            result.DefeatWasPrevented);

        Assert.False(
            result.DefeatWasAccepted);

        Assert.False(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.Equal(
            25d,
            setup.LifeTarget.State.Current);

        // Same concrete ResourceState:
        // final state committed once.
        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        // Gross operations remain separate.
        Assert.Equal(
            2,
            ledger.Count);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            ledger.Entries[0].Provenance.Cause);

        Assert.Equal(
            ResourceOperationCause.Recovery,
            ledger.Entries[1].Provenance.Cause);
    }

    [Fact]
    public void StaleUnresolvedPhaseResult_IsRejectedIfDraftWasLaterRescued()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var stalePhaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Unresolved,
            stalePhaseResult.Outcome);

        // Draft changes after finalization.
        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    stalePhaseResult));

        // Still no visible commit.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        Assert.Equal(
            25d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);
    }

    [Fact]
    public void PhaseResultFromDifferentDraft_IsRejectedBeforeCommit()
    {
        var setup =
            CreateSetup();

        var firstDraft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            firstDraft,
            setup,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                firstDraft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        var secondDraft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            secondDraft,
            setup,
            amount: 100d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    secondDraft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    phaseResult));

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void DifferentPolicyPhaseResult_IsRejectedBeforeCommit()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLifeLoss(
            draft,
            setup,
            amount: 100d);

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

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AllDepleted,
                    phaseResult));

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void EntityFromDifferentRegistry_IsRejectedBeforeCommit()
    {
        var first =
            CreateSetup();

        var second =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                first.Registry);

        StageLifeLoss(
            draft,
            first,
            amount: 50d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<ArgumentException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    second.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        Assert.Equal(
            100d,
            first.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            first.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }

    private static void StageLifeLoss(
        ResourceTransactionDraft draft,
        TestSetup setup,
        double amount)
    {
        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));
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
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.soul"),
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var soulId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.soul"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d),

                    new ResourceState(
                        soulId,
                        current: 50d,
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