using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceTransactionLedgerEntryFactoryTests
{
    [Fact]
    public void EmptyDraft_ProducesNoEntries()
    {
        var draft =
            new ResourceTransactionDraft(
                CreateRegistry());

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        Assert.Empty(
            entries);
    }

    [Fact]
    public void CreateEntries_DoesNotMutateOriginalState()
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
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        Assert.Single(
            entries);

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void CreateEntries_PreservesOperationOrder()
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
            new ResourceLossRequest(
                lifeId,
                30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

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

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        Assert.Equal(
            3,
            entries.Count);

        Assert.IsType<ResourceLossLedgerEntry>(
            entries[0]);

        Assert.IsType<ResourceCostLedgerEntry>(
            entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            entries[2]);
    }

    [Fact]
    public void LossEntry_PreservesExactStagedRequestAndResult()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var state =
            new ResourceState(
                lifeId,
                current: 40d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                lifeId,
                100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var draft =
            new ResourceTransactionDraft(
                registry);

        var preview =
            draft.StageLoss(
                state,
                request,
                preventedLoss: 20d);

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                entries[0]);

        Assert.Equal(
            request,
            entry.Request);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            100d,
            entry.Result.RequestedLoss);

        Assert.Equal(
            20d,
            entry.Result.PreventedLoss);

        Assert.Equal(
            40d,
            entry.Result.ActualLoss);

        Assert.Equal(
            40d,
            entry.Result.Shortfall);
    }

    [Fact]
    public void CostEntry_ContainsSuccessfulQuantitativeResult()
    {
        var registry =
            CreateRegistry();

        var manaId =
            GetManaId(
                registry);

        var state =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var request =
            new ResourceCostRequest(
                manaId,
                20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageCost(
            state,
            request);

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        var entry =
            Assert.IsType<ResourceCostLedgerEntry>(
                entries[0]);

        Assert.Equal(
            request,
            entry.Request);

        Assert.Equal(
            20d,
            entry.Result.RequestedCost);

        Assert.Equal(
            20d,
            entry.Result.ActualCost);

        Assert.Equal(
            ResourceOperationCause.SkillCost,
            entry.Provenance.Cause);
    }

    [Fact]
    public void RecoveryEntry_PreservesExactStagedRequestAndResult()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var state =
            new ResourceState(
                lifeId,
                current: 80d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                lifeId,
                50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Leech));

        var draft =
            new ResourceTransactionDraft(
                registry);

        var preview =
            draft.StageRecovery(
                state,
                request);

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        var entry =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                entries[0]);

        Assert.Equal(
            request,
            entry.Request);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            20d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            30d,
            entry.Result.Overflow);
    }

    [Fact]
    public void MultipleOperationsOnSameState_ProduceSeparateGrossEntries()
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

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        Assert.Equal(
            3,
            entries.Count);

        var firstLoss =
            Assert.IsType<ResourceLossLedgerEntry>(
                entries[0]);

        var secondLoss =
            Assert.IsType<ResourceLossLedgerEntry>(
                entries[1]);

        var recovery =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                entries[2]);

        Assert.Equal(
            30d,
            firstLoss.Result.ActualLoss);

        Assert.Equal(
            20d,
            secondLoss.Result.ActualLoss);

        Assert.Equal(
            10d,
            recovery.Result.ActualRecovery);

        // Final projected state:
        // 100 - 30 - 20 + 10 = 60
        Assert.Equal(
            60d,
            draft.Projections[0].Current);

        // Preparing ledger entries must still not
        // mutate the original runtime state.
        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void ReturnedCollection_IsReadOnly()
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
                10d));

        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        var list =
            Assert.IsAssignableFrom<
                IList<ResourceOperationLedgerEntry>>(
                entries);

        Assert.Throws<NotSupportedException>(
            () =>
                list.Add(
                    entries[0]));
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