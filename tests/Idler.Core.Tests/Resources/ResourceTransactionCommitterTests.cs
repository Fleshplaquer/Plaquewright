using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceTransactionCommitterTests
{
    [Fact]
    public void Commit_EmptyDraft_IsNoOp()
    {
        var draft =
            new ResourceTransactionDraft(
                CreateRegistry());

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void Commit_PublishesStateAndLedgerEntries()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            new ResourceLossRequest(
                lifeId,
                30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            70d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            30d,
            entry.Result.ActualLoss);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            entry.Provenance.Cause);
    }

    [Fact]
    public void MultipleOperationsOnSameState_CreateGrossEntriesButOneStateRevision()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                30d));

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                20d));

        draft.StageRecovery(
            state,
            new ResourceRecoveryRequest(
                lifeId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            60d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            3,
            ledger.Count);

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[0]);

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            ledger.Entries[2]);
    }

    [Fact]
    public void Commit_PreservesStageOrderAcrossResourcesAndOperationTypes()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var manaId =
            GetManaId(
                registry);

        var life =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var mana =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            life,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            mana,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        draft.StageRecovery(
            life,
            new ResourceRecoveryRequest(
                lifeId,
                5d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Leech)));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            80d,
            life.Current);

        Assert.Equal(
            40d,
            mana.Current);

        Assert.Equal(
            3,
            ledger.Count);

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[0]);

        Assert.IsType<ResourceCostLedgerEntry>(
            ledger.Entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            ledger.Entries[2]);
    }

    [Fact]
    public void StaleProjection_PreventsStateAndLedgerPublication()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var manaId =
            GetManaId(
                registry);

        var life =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var mana =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            life,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            mana,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        // Make the later projection stale.
        mana.SetValues(
            current: 40d,
            maximum: 50d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionCommitter.Commit(
                    draft,
                    ledger));

        // Life must remain completely untouched.
        Assert.Equal(
            100d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);

        // Only the explicit external mutation remains.
        Assert.Equal(
            40d,
            mana.Current);

        Assert.Equal(
            1UL,
            mana.Revision);

        // No transaction operations became visible.
        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void Commit_AppendsAfterExistingLedgerEntries()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

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
                existingState,
                existingPreview);

        var ledger =
            new ResourceOperationLedger();

        ledger.Append(
            existingEntry);

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                10d));

        draft.StageRecovery(
            state,
            new ResourceRecoveryRequest(
                lifeId,
                5d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            3,
            ledger.Count);

        Assert.Same(
            existingEntry,
            ledger.Entries[0]);

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            ledger.Entries[2]);
    }

    [Fact]
    public void SecondCommitOfSameDraft_FailsWithoutDuplicateLedgerEntries()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                25d));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionCommitter.Commit(
                    draft,
                    ledger));

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            1,
            ledger.Count);
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