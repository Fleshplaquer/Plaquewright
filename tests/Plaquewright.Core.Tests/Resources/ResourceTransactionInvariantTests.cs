using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionInvariantTests
{
    [Fact]
    public void SameResourceIdOnDifferentStates_CommitsIndependently()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateLifeEntity(
                registry,
                1UL,
                100d,
                100d);

        var secondEntity =
            CreateLifeEntity(
                registry,
                2UL,
                200d,
                200d);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var first =
            firstTarget.State;

        var second =
            secondTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            firstTarget,
            CreateLossRequest(
                lifeId,
                10d));

        draft.StageLoss(
            secondTarget,
            CreateLossRequest(
                lifeId,
                20d));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            90d,
            first.Current);

        Assert.Equal(
            180d,
            second.Current);

        Assert.Equal(
            1UL,
            first.Revision);

        Assert.Equal(
            1UL,
            second.Revision);

        Assert.Equal(
            2,
            ledger.Count);
    }

    [Fact]
    public void FailedTransaction_PreservesExistingLedgerPrefixExactly()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var manaId =
            GetManaId(
                registry);

        // Existing low-level entry remains independent
        // from the owner-aware transaction being tested.
        var existingState =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var existingPreview =
            ResourceLossOperations.Preview(
                existingState,
                CreateLossRequest(
                    lifeId,
                    5d));

        var existingEntry =
    ResourceLossOperations.Commit(
        new EntityId(999UL),
        existingState,
        existingPreview);

        var ledger =
            new ResourceOperationLedger();

        ledger.Append(
            existingEntry);

        var entity =
            CreateLifeManaEntity(
                registry,
                1UL,
                100d,
                100d,
                50d,
                50d);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var life =
            lifeTarget.State;

        var mana =
            manaTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            lifeTarget,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        mana.SetValues(
            current: 40d,
            maximum: 50d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionCommitter.Commit(
                    draft,
                    ledger));

        Assert.Equal(
            100d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        Assert.Same(
            existingEntry,
            ledger.Entries[0]);
    }

    [Fact]
    public void GrossLedgerAccountingAndFinalProjectedState_RemainConsistent()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                1UL,
                70d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var life =
            target.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            new ResourceLossRequest(
                lifeId,
                100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)),
            preventedLoss: 20d);

        draft.StageRecovery(
            target,
            new ResourceRecoveryRequest(
                lifeId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Leech)));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            25d,
            life.Current);

        Assert.Equal(
            1UL,
            life.Revision);

        Assert.Equal(
            2,
            ledger.Count);

        var loss =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        var recovery =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            100d,
            loss.Result.RequestedLoss);

        Assert.Equal(
            20d,
            loss.Result.PreventedLoss);

        Assert.Equal(
            70d,
            loss.Result.ActualLoss);

        Assert.Equal(
            10d,
            loss.Result.Shortfall);

        Assert.Equal(
            25d,
            recovery.Result.RequestedRecovery);

        Assert.Equal(
            25d,
            recovery.Result.ActualRecovery);

        Assert.Equal(
            0d,
            recovery.Result.Overflow);
    }

    private static EntityRuntimeState CreateLifeEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double current,
        double maximum)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    current,
                    maximum)
            ]);
    }

    private static EntityRuntimeState CreateLifeManaEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double lifeCurrent,
        double lifeMaximum,
        double manaCurrent,
        double manaMaximum)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    lifeCurrent,
                    lifeMaximum),

                new ResourceState(
                    GetManaId(registry),
                    manaCurrent,
                    manaMaximum)
            ]);
    }

    private static ResourceLossRequest CreateLossRequest(
        ResourceId resourceId,
        double amount)
    {
        return new ResourceLossRequest(
            resourceId,
            amount,
            new ResourceOperationProvenance(
                ResourceOperationCause.Direct));
    }

    private static ResourceId GetLifeId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.life"));
    }

    private static ResourceId GetManaId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.mana"));
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
                    "resource.mana"),
                ResourceRole.CostSource)
        ]);
    }
}