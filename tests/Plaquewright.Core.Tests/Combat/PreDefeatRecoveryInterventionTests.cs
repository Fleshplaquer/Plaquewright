using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class PreDefeatRecoveryInterventionTests
{
    [Fact]
    public void Recovery_CanResolveProjectedAnyPolicyDefeat()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.Equal(
            setup.LifeId,
            result.ResourceId);

        Assert.Equal(
            25d,
            result.RequestedRecovery);

        Assert.Equal(
            0d,
            result.ProjectedCurrentBefore);

        Assert.Equal(
            25d,
            result.ProjectedCurrentAfter);

        Assert.Equal(
            25d,
            result.ActualProjectedRecovery);

        Assert.True(
            result.EvaluationBefore.IsProjectedDefeated);

        Assert.False(
            result.EvaluationAfter.IsProjectedDefeated);

        Assert.True(
            result.WasDefeatResolved);

        Assert.False(
            result.IsStillProjectedDefeated);

        var projected =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            25d,
            projected.Current);

        // Still projected only.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            2,
            draft.OperationCount);
    }

    [Fact]
    public void Recovery_CanResolveAllPolicyByRestoringOneRelevantResource()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        StageLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 1d);

        Assert.True(
            result.EvaluationBefore.IsProjectedDefeated);

        Assert.False(
            result.EvaluationAfter.IsProjectedDefeated);

        Assert.True(
            result.WasDefeatResolved);

        Assert.Equal(
            1d,
            result.ProjectedCurrentAfter);

        // Soul remains projected depleted.
        var soul =
            draft.GetProjectedValues(
                setup.SoulTarget);

        Assert.Equal(
            0d,
            soul.Current);
    }

    [Fact]
    public void Recovery_MayLeaveEntityProjectedDefeated()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        StageLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        // Life is rescued...
        Assert.Equal(
            25d,
            result.ProjectedCurrentAfter);

        // ...but Soul remains at zero, so ANY policy
        // still considers the entity defeated.
        Assert.True(
            result.EvaluationAfter.IsProjectedDefeated);

        Assert.False(
            result.WasDefeatResolved);

        Assert.True(
            result.IsStillProjectedDefeated);
    }

    [Fact]
    public void Recovery_IsClampedByProjectedMaximum()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 250d);

        Assert.Equal(
            250d,
            result.RequestedRecovery);

        Assert.Equal(
            0d,
            result.ProjectedCurrentBefore);

        Assert.Equal(
            100d,
            result.ProjectedCurrentAfter);

        Assert.Equal(
            100d,
            result.ActualProjectedRecovery);

        Assert.True(
            result.WasDefeatResolved);
    }

    [Fact]
    public void NonDefeatRelevantResource_IsRejectedWithoutStaging()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var operationCountBefore =
            draft.OperationCount;

        Assert.Throws<InvalidOperationException>(
            () =>
                PreDefeatRecoveryIntervention.Apply(
                    context,
                    setup.ManaId,
                    recoveryAmount: 20d));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            40d,
            setup.ManaTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.ManaTarget.State.Revision);
    }

    [Fact]
    public void ContextWhoseDraftWasAlreadyRescued_IsRejectedWithoutAdditionalStaging()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        // Some earlier intervention already rescues Life.
        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        Assert.Equal(
            2,
            draft.OperationCount);

        Assert.Throws<InvalidOperationException>(
            () =>
                PreDefeatRecoveryIntervention.Apply(
                    context,
                    setup.LifeId,
                    recoveryAmount: 10d));

        // No third operation was staged.
        Assert.Equal(
            2,
            draft.OperationCount);

        var projected =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            25d,
            projected.Current);

        // Trigger remains historical.
        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void RecoveryAndOriginalLoss_CommitAtomicallyAsGrossOperations()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.True(
            result.WasDefeatResolved);

        // Nothing visible before commit.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        // Final projected state is committed once.
        Assert.Equal(
            25d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        // But both causal gross operations remain visible.
        Assert.Equal(
            2,
            ledger.Count);

        Assert.Equal(
            setup.Entity.Id,
            ledger.Entries[0].TargetEntityId);

        Assert.Equal(
            setup.LifeId,
            ledger.Entries[0].ResourceId);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            ledger.Entries[0].Provenance.Cause);

        Assert.Equal(
            setup.Entity.Id,
            ledger.Entries[1].TargetEntityId);

        Assert.Equal(
            setup.LifeId,
            ledger.Entries[1].ResourceId);

        Assert.Equal(
            ResourceOperationCause.Recovery,
            ledger.Entries[1].Provenance.Cause);

        // Other resource remains untouched.
        Assert.Equal(
            50d,
            setup.SoulTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.SoulTarget.State.Revision);
    }

    [Fact]
    public void InvalidRecoveryAmount_DoesNotModifyDraft()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var operationCountBefore =
            draft.OperationCount;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                PreDefeatRecoveryIntervention.Apply(
                    context,
                    setup.LifeId,
                    recoveryAmount: -1d));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        var projected =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            0d,
            projected.Current);
    }

    private static void StageLoss(
        ResourceTransactionDraft draft,
        ResourceStateTarget target,
        ResourceId resourceId,
        double amount)
    {
        draft.StageLoss(
            target,
            new ResourceLossRequest(
                resourceId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));
    }

    private static TestSetup CreateSetup(
        double currentLife,
        double currentSoul,
        double currentMana)
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
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource |
                    ResourceRole.ProtectionSource)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var soulId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.soul"));

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
                        lifeId,
                        currentLife,
                        maximum: 100d),

                    new ResourceState(
                        soulId,
                        currentSoul,
                        maximum: 100d),

                    new ResourceState(
                        manaId,
                        currentMana,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            lifeId,
            soulId,
            manaId,
            entity,
            new ResourceStateTarget(
                entity,
                lifeId),
            new ResourceStateTarget(
                entity,
                soulId),
            new ResourceStateTarget(
                entity,
                manaId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId SoulId,
        ResourceId ManaId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget SoulTarget,
        ResourceStateTarget ManaTarget);
}