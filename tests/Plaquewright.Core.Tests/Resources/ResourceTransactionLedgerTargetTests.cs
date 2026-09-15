using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionLedgerTargetTests
{
    [Fact]
    public void TransactionLedgerEntry_PreservesTargetEntityId()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            new EntityRuntimeState(
                new EntityId(42UL),
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

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            new ResourceLossRequest(
                lifeId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            new EntityId(42UL),
            entry.TargetEntityId);

        Assert.Equal(
            lifeId,
            entry.ResourceId);
    }

    [Fact]
    public void SameResourceIdOnDifferentEntities_RemainsDistinctInLedger()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateEntity(
                registry,
                entityId: 1UL);

        var secondEntity =
            CreateEntity(
                registry,
                entityId: 2UL);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

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
            2,
            ledger.Count);

        Assert.Equal(
            lifeId,
            ledger.Entries[0].ResourceId);

        Assert.Equal(
            lifeId,
            ledger.Entries[1].ResourceId);

        Assert.Equal(
            new EntityId(1UL),
            ledger.Entries[0].TargetEntityId);

        Assert.Equal(
            new EntityId(2UL),
            ledger.Entries[1].TargetEntityId);

        Assert.NotEqual(
            ledger.Entries[0].TargetEntityId,
            ledger.Entries[1].TargetEntityId);
    }

    [Fact]
    public void GrossOperationsOnSameTarget_AllPreserveSameEntityId()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateEntity(
                registry,
                entityId: 17UL);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            CreateLossRequest(
                lifeId,
                30d));

        draft.StageRecovery(
            target,
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
            2,
            ledger.Count);

        Assert.All(
            ledger.Entries,
            entry =>
                Assert.Equal(
                    new EntityId(17UL),
                    entry.TargetEntityId));
    }

    private static EntityRuntimeState CreateEntity(
        CompiledResourceRegistry registry,
        ulong entityId)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    current: 100d,
                    maximum: 100d)
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

    private static CompiledResourceRegistry CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
        ]);
    }
}