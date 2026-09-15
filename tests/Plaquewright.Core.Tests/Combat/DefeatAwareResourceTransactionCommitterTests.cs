using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

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
    public void PhaseResult_IsRejectedWhenDraftChangedEvenIfDefeatBooleansRemainEqual()
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

        Assert.True(
            stalePhaseResult.FinalEvaluation.IsProjectedDefeated);

        Assert.True(
            stalePhaseResult.FinalEvaluation.IsNewDefeatTransition);

        var soulId =
            setup.Registry.GetId(
                ResourceKey.Parse(
                    "resource.soul"));

        var soulTarget =
            new ResourceStateTarget(
                setup.Entity,
                soulId);

        // Change the draft after finalization without changing
        // the relevant defeat booleans:
        //
        // Life remains projected at zero.
        // Soul merely drops from 50 to 40.
        draft.StageLoss(
            soulTarget,
            new ResourceLossRequest(
                soulId,
                amount: 10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct)));

        var currentObservation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var currentEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                currentObservation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        // This is the exact A02 regression:
        // the old boolean-only validation would consider
        // these equivalent.
        Assert.Equal(
            stalePhaseResult.FinalEvaluation.IsProjectedDefeated,
            currentEvaluation.IsProjectedDefeated);

        Assert.Equal(
            stalePhaseResult.FinalEvaluation.IsNewDefeatTransition,
            currentEvaluation.IsNewDefeatTransition);

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

        // Nothing became visible.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            50d,
            soulTarget.State.Current);

        Assert.Equal(
            0UL,
            soulTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        // But both staged projections are still present.
        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        Assert.Equal(
            40d,
            draft.GetProjectedValues(
                soulTarget)
            .Current);
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
            CreateRegistry();

        return CreateSetup(
            registry,
            new EntityId(1UL));
    }

    private static TestSetup CreateSetup(
        CompiledResourceRegistry registry,
        EntityId entityId)
    {
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
                entityId,
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

    [Fact]
    public void MultiOwnerDraft_WithoutDefeatTransitions_CommitsAllOwnersAtomically()
    {
        var registry =
            CreateRegistry();

        var first =
            CreateSetup(
                registry,
                new EntityId(1UL));

        var second =
            CreateSetup(
                registry,
                new EntityId(2UL));

        var draft =
            new ResourceTransactionDraft(
                registry);

        StageLifeLoss(
            draft,
            first,
            amount: 40d);

        StageLifeLoss(
            draft,
            second,
            amount: 30d);

        var ledger =
            new ResourceOperationLedger();

        var results =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                [
                    new DefeatAwareResourceTransactionOwnerCommitRequest(
                    first.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted),

                new DefeatAwareResourceTransactionOwnerCommitRequest(
                    second.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted)
                ]);

        Assert.Equal(
            2,
            results.Count);

        Assert.Equal(
            first.Entity.Id,
            results[0].EntityId);

        Assert.Equal(
            second.Entity.Id,
            results[1].EntityId);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition,
            results[0].Outcome);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition,
            results[1].Outcome);

        Assert.Equal(
            60d,
            first.LifeTarget.State.Current);

        Assert.Equal(
            70d,
            second.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            first.LifeTarget.State.Revision);

        Assert.Equal(
            1UL,
            second.LifeTarget.State.Revision);

        Assert.Equal(
            2,
            ledger.Count);
    }

    [Fact]
    public void MultiOwnerDraft_MissingPhaseForOneDefeatedOwner_RejectsEntireCommit()
    {
        var registry =
            CreateRegistry();

        var first =
            CreateSetup(
                registry,
                new EntityId(1UL));

        var second =
            CreateSetup(
                registry,
                new EntityId(2UL));

        var draft =
            new ResourceTransactionDraft(
                registry);

        StageLifeLoss(
            draft,
            first,
            amount: 100d);

        StageLifeLoss(
            draft,
            second,
            amount: 100d);

        var firstContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                first.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            firstContext);

        var firstPhaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                firstContext);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    [
                        new DefeatAwareResourceTransactionOwnerCommitRequest(
                        first.Entity,
                        DefeatRelevantResourcePolicy.AnyDepleted,
                        firstPhaseResult),

                    new DefeatAwareResourceTransactionOwnerCommitRequest(
                        second.Entity,
                        DefeatRelevantResourcePolicy.AnyDepleted)
                    ]));

        // Owner A passed its gate, but Owner B did not.
        // Therefore neither becomes visible.
        Assert.Equal(
            100d,
            first.LifeTarget.State.Current);

        Assert.Equal(
            100d,
            second.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            first.LifeTarget.State.Revision);

        Assert.Equal(
            0UL,
            second.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void MultiOwnerDraft_WithPhaseForEveryDefeatedOwner_CommitsOnceAllOwnersPass()
    {
        var registry =
            CreateRegistry();

        var first =
            CreateSetup(
                registry,
                new EntityId(1UL));

        var second =
            CreateSetup(
                registry,
                new EntityId(2UL));

        var draft =
            new ResourceTransactionDraft(
                registry);

        StageLifeLoss(
            draft,
            first,
            amount: 100d);

        StageLifeLoss(
            draft,
            second,
            amount: 100d);

        var firstContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                first.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        var secondContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                second.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            firstContext);

        Assert.NotNull(
            secondContext);

        // No draft mutation occurs between these finalizations,
        // so both results bind to the same current draft version.
        var firstPhaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                firstContext);

        var secondPhaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                secondContext);

        var ledger =
            new ResourceOperationLedger();

        var results =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                [
                    new DefeatAwareResourceTransactionOwnerCommitRequest(
                    first.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    firstPhaseResult),

                new DefeatAwareResourceTransactionOwnerCommitRequest(
                    second.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    secondPhaseResult)
                ]);

        Assert.Equal(
            2,
            results.Count);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted,
            results[0].Outcome);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted,
            results[1].Outcome);

        Assert.Equal(
            0d,
            first.LifeTarget.State.Current);

        Assert.Equal(
            0d,
            second.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            first.LifeTarget.State.Revision);

        Assert.Equal(
            1UL,
            second.LifeTarget.State.Revision);

        Assert.Equal(
            2,
            ledger.Count);
    }

    [Fact]
    public void MultiOwnerDraft_SameIdDifferentEntity_DoesNotSatisfyOwnerGate()
    {
        var registry =
            CreateRegistry();

        var actualOwner =
            CreateSetup(
                registry,
                new EntityId(1UL));

        var sameIdImposter =
            CreateSetup(
                registry,
                new EntityId(1UL));

        var draft =
            new ResourceTransactionDraft(
                registry);

        StageLifeLoss(
            draft,
            actualOwner,
            amount: 40d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    [
                        new DefeatAwareResourceTransactionOwnerCommitRequest(
                        sameIdImposter.Entity,
                        DefeatRelevantResourcePolicy.AnyDepleted)
                    ]));

        Assert.Equal(
            100d,
            actualOwner.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            actualOwner.LifeTarget.State.Revision);

        Assert.Equal(
            100d,
            sameIdImposter.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            sameIdImposter.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }
    [Fact]
    public void MultiOwnerDraft_OwnerWithoutDefeatRelevantResources_DoesNotRequirePreDefeatPhase()
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
                    "resource.mana"),
                ResourceRole.CostSource)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var lifeTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var manaTarget =
            new ResourceStateTarget(
                secondEntity,
                manaId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            lifeTarget,
            new ResourceLossRequest(
                lifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                amount: 10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        var firstContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                firstEntity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            firstContext);

        var firstPhaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                firstContext);

        var ledger =
            new ResourceOperationLedger();

        var results =
            DefeatAwareResourceTransactionCommitter.Commit(
                draft,
                ledger,
                [
                    new DefeatAwareResourceTransactionOwnerCommitRequest(
                    firstEntity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    firstPhaseResult),

                new DefeatAwareResourceTransactionOwnerCommitRequest(
                    secondEntity,
                    DefeatRelevantResourcePolicy.AnyDepleted)
                ]);

        Assert.Equal(
            2,
            results.Count);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted,
            results[0].Outcome);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition,
            results[1].Outcome);

        Assert.False(
            results[1].FinalEvaluation.IsProjectedDefeated);

        Assert.False(
            results[1].FinalEvaluation.IsNewDefeatTransition);

        Assert.Equal(
            0d,
            lifeTarget.State.Current);

        Assert.Equal(
            40d,
            manaTarget.State.Current);

        Assert.Equal(
            1UL,
            lifeTarget.State.Revision);

        Assert.Equal(
            1UL,
            manaTarget.State.Revision);

        Assert.Equal(
            2,
            ledger.Count);
    }

    private static CompiledResourceRegistry CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
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
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget);
}