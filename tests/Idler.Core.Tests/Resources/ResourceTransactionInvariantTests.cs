using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

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

        var first =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var second =
            new ResourceState(
                lifeId,
                current: 200d,
                maximum: 200d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            first,
            CreateLossRequest(
                lifeId,
                10d));

        draft.StageLoss(
            second,
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

        // Make transaction stale before commit.
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

        var life =
            new ResourceState(
                lifeId,
                current: 70d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            life,
            new ResourceLossRequest(
                lifeId,
                100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)),
            preventedLoss: 20d);

        draft.StageRecovery(
            life,
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