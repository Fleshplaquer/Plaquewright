using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationProvenanceFlowTests
{
    [Fact]
    public void LossPreview_PreservesDamageDerivedProvenance()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 25d,
                provenance);

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        Assert.Equal(
            request,
            preview.Request);

        Assert.Equal(
            provenance,
            preview.Request.Provenance);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            preview.Request.Provenance.Cause);
    }

    [Fact]
    public void TransactionDraftAndLedger_PreservePerOperationGameplayExecutions()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget),

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
                    manaId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var lossExecutionId =
            new ExecutionId(11UL);

        var costExecutionId =
            new ExecutionId(12UL);

        var recoveryExecutionId =
            new ExecutionId(13UL);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            lifeTarget,
            new ResourceLossRequest(
                lifeId,
                amount: 30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived,
                    lossExecutionId)));

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                amount: 20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost,
                    costExecutionId)));

        draft.StageRecovery(
            lifeTarget,
            new ResourceRecoveryRequest(
                lifeId,
                amount: 10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery,
                    recoveryExecutionId)));

        Assert.Equal(
            3,
            draft.OperationCount);

        Assert.Equal(
            lossExecutionId,
            draft.Operations[0].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        Assert.Equal(
            costExecutionId,
            draft.Operations[1].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        Assert.Equal(
            recoveryExecutionId,
            draft.Operations[2].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            3,
            ledger.Count);

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[0]);

        Assert.IsType<ResourceCostLedgerEntry>(
            ledger.Entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            ledger.Entries[2]);

        Assert.Equal(
            lossExecutionId,
            ledger.Entries[0].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        Assert.Equal(
            costExecutionId,
            ledger.Entries[1].Provenance.GameplayExecutionId
                .GetValueOrDefault());

        Assert.Equal(
            recoveryExecutionId,
            ledger.Entries[2].Provenance.GameplayExecutionId
                .GetValueOrDefault());
    }

    [Fact]
    public void CostPreview_PreservesSkillCostProvenance()
    {
        var key =
            ResourceKey.Parse(
                "resource.mana");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    key,
                    ResourceRole.CostSource)
            ]);

        var state =
            new ResourceState(
                registry.GetId(key),
                current: 100d,
                maximum: 100d);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.SkillCost);

        var request =
            new ResourceCostRequest(
                state.Id,
                amount: 40d,
                provenance);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                request);

        Assert.Equal(
            request,
            preview.Request);

        Assert.Equal(
            provenance,
            preview.Request.Provenance);

        Assert.Equal(
            ResourceOperationCause.SkillCost,
            preview.Request.Provenance.Cause);
    }

    [Fact]
    public void RecoveryPreview_PreservesLeechProvenance()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.Leech);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 20d,
                provenance);

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        Assert.Equal(
            request,
            preview.Request);

        Assert.Equal(
            provenance,
            preview.Request.Provenance);

        Assert.Equal(
            ResourceOperationCause.Leech,
            preview.Request.Provenance.Cause);
    }

    [Fact]
    public void LossCommit_DoesNotLoseAccessToOriginalProvenance()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceLossRequest(
                lifeId,
                amount: 30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Sacrifice));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceLossOperations.Commit(
                target,
                preview);

        Assert.Equal(
            ResourceOperationCause.Sacrifice,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            30d,
            entry.Result.ActualLoss);

        Assert.Equal(
            70d,
            target.State.Current);
    }

    [Fact]
    public void CostCommit_DoesNotLoseAccessToOriginalProvenance()
    {
        var key =
            ResourceKey.Parse(
                "resource.mana");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                key,
                ResourceRole.CostSource)
            ]);

        var manaId =
            registry.GetId(
                key);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var request =
            new ResourceCostRequest(
                manaId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                registry,
                target.State,
                request);

        var entry =
            ResourceCostOperations.Commit(
                target,
                preview);

        Assert.Equal(
            ResourceOperationCause.SkillCost,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            25d,
            entry.Result.ActualCost);

        Assert.Equal(
            75d,
            target.State.Current);
    }



    [Fact]
    public void SameQuantityWithDifferentProvenance_RemainsSemanticallyDistinct()
    {
        var resourceId =
            new ResourceId(1);

        var damage =
            new ResourceLossRequest(
                resourceId,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var sacrifice =
            new ResourceLossRequest(
                resourceId,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Sacrifice));

        Assert.Equal(
            damage.ResourceId,
            sacrifice.ResourceId);

        Assert.Equal(
            damage.Amount,
            sacrifice.Amount);

        Assert.NotEqual(
            damage,
            sacrifice);

        Assert.NotEqual(
            damage.Provenance,
            sacrifice.Provenance);
    }
    [Fact]
    public void RecoveryCommit_DoesNotLoseAccessToOriginalProvenance()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceRecoveryRequest(
                lifeId,
                amount: 20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Regeneration));

        var preview =
            ResourceRecoveryOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                target,
                preview);

        Assert.Equal(
            ResourceOperationCause.Regeneration,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            20d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            70d,
            target.State.Current);
    }
}