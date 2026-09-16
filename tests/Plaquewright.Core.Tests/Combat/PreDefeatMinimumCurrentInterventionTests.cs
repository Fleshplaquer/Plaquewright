using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class PreDefeatMinimumCurrentInterventionTests
{
    [Fact]
    public void MinimumCurrent_RaisesDepletedResourceToOne()
    {
        var setup =
            CreateSetup();

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
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 1d);

        Assert.Equal(
            setup.LifeId,
            result.ResourceId);

        Assert.Equal(
            1d,
            result.MinimumCurrent);

        Assert.Equal(
            1d,
            result.RequestedRecovery);

        Assert.Equal(
            0d,
            result.ProjectedCurrentBefore);

        Assert.Equal(
            1d,
            result.ProjectedCurrentAfter);

        Assert.Equal(
            1d,
            result.ActualProjectedRecovery);

        Assert.True(
            result.DidStageRecovery);

        Assert.True(
            result.WasDefeatResolved);

        Assert.False(
            result.IsStillProjectedDefeated);

        Assert.Equal(
            2,
            draft.OperationCount);

        // Still projected only.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void MinimumCurrent_StagesOnlyMissingAmount()
    {
        var setup =
            CreateSetup(
                currentLife: 10d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 9.5d);

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
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 1d);

        Assert.Equal(
            0.5d,
            result.ProjectedCurrentBefore);

        Assert.Equal(
            0.5d,
            result.RequestedRecovery);

        Assert.Equal(
            1d,
            result.ProjectedCurrentAfter);

        Assert.Equal(
            0.5d,
            result.ActualProjectedRecovery);

        Assert.True(
            result.DidStageRecovery);

        // Soul is still depleted.
        Assert.True(
            result.IsStillProjectedDefeated);

        Assert.False(
            result.WasDefeatResolved);

        Assert.Equal(
            3,
            draft.OperationCount);

        var operation =
            Assert.IsType<StagedResourceRecoveryOperation>(
                draft.Operations[2]);

        Assert.Equal(
            0.5d,
            operation.Preview.Result.RequestedRecovery);

        Assert.Equal(
            0.5d,
            operation.Preview.Result.ActualRecovery);
    }

    [Fact]
    public void MultipleMinimumCurrentInterventions_CanResolveAnyPolicyDefeatSequentially()
    {
        var setup =
            CreateSetup();

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

        var lifeResult =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 1d);

        Assert.False(
            lifeResult.WasDefeatResolved);

        Assert.True(
            lifeResult.IsStillProjectedDefeated);

        var soulResult =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.SoulId,
                minimumCurrent: 1d);

        Assert.True(
            soulResult.WasDefeatResolved);

        Assert.False(
            soulResult.IsStillProjectedDefeated);

        Assert.Equal(
            1d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);

        Assert.Equal(
            1d,
            draft.GetProjectedValues(
                setup.SoulTarget)
            .Current);

        Assert.Equal(
            4,
            draft.OperationCount);
    }

    [Fact]
    public void MinimumAboveProjectedMaximum_IsRejectedWithoutStaging()
    {
        var setup =
            CreateSetup();

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

        var versionBefore =
            draft.Version;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                PreDefeatMinimumCurrentIntervention.Apply(
                    context,
                    setup.LifeId,
                    minimumCurrent: 101d));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidMinimumCurrent_IsRejectedWithoutStaging(
        double minimumCurrent)
    {
        var setup =
            CreateSetup();

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

        var versionBefore =
            draft.Version;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                PreDefeatMinimumCurrentIntervention.Apply(
                    context,
                    setup.LifeId,
                    minimumCurrent));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            versionBefore,
            draft.Version);
    }

    [Fact]
    public void NonDefeatRelevantResource_IsRejectedWithoutStaging()
    {
        var setup =
            CreateSetup();

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

        var versionBefore =
            draft.Version;

        Assert.Throws<InvalidOperationException>(
            () =>
                PreDefeatMinimumCurrentIntervention.Apply(
                    context,
                    setup.ManaId,
                    minimumCurrent: 1d));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Equal(
            40d,
            setup.ManaTarget.State.Current);
    }

    [Fact]
    public void ContextWhoseDraftWasAlreadyRescued_IsRejectedWithoutAdditionalStaging()
    {
        var setup =
            CreateSetup();

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

        var firstResult =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 1d);

        Assert.True(
            firstResult.WasDefeatResolved);

        var operationCountBefore =
            draft.OperationCount;

        var versionBefore =
            draft.Version;

        Assert.Throws<InvalidOperationException>(
            () =>
                PreDefeatMinimumCurrentIntervention.Apply(
                    context,
                    setup.LifeId,
                    minimumCurrent: 2d));

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Equal(
            1d,
            draft.GetProjectedValues(
                setup.LifeTarget)
            .Current);
    }

    [Fact]
    public void ResourceAlreadyAtMinimum_DoesNotStageNoOpRecovery()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

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

        var operationCountBefore =
            draft.OperationCount;

        var versionBefore =
            draft.Version;

        var result =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 50d);

        Assert.Equal(
            100d,
            result.ProjectedCurrentBefore);

        Assert.Equal(
            100d,
            result.ProjectedCurrentAfter);

        Assert.Equal(
            0d,
            result.RequestedRecovery);

        Assert.Equal(
            0d,
            result.ActualProjectedRecovery);

        Assert.False(
            result.DidStageRecovery);

        Assert.True(
            result.IsStillProjectedDefeated);

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            versionBefore,
            draft.Version);
    }

    [Fact]
    public void MinimumCurrentIntervention_CommitsRecoveryBasedMinimumThroughDefeatAwareLifecycle()
    {
        var setup =
            CreateSetup();

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

        var intervention =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 1d);

        Assert.True(
            intervention.WasDefeatResolved);

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
            1d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            2,
            ledger.Count);

        var lossEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            lossEntry.Provenance.Cause);

        Assert.Equal(
            100d,
            lossEntry.Result.RequestedLoss);

        Assert.Equal(
            100d,
            lossEntry.Result.ActualLoss);

        var recoveryEntry =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            ResourceOperationCause.Recovery,
            recoveryEntry.Provenance.Cause);

        Assert.Equal(
            1d,
            recoveryEntry.Result.RequestedRecovery);

        Assert.Equal(
            1d,
            recoveryEntry.Result.ActualRecovery);
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
        double currentLife = 100d,
        double currentSoul = 50d,
        double currentMana = 40d)
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