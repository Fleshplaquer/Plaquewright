using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationLedgerTests
{
    [Fact]
    public void NewLedger_IsEmpty()
    {
        var ledger =
            new ResourceOperationLedger();

        Assert.Equal(
            0,
            ledger.Count);

        Assert.Empty(
            ledger.Entries);
    }

    [Fact]
    public void Append_PreservesCommittedEntry()
    {
        var entry =
            CreateLossEntry(
                amount: 25d);

        var ledger =
            new ResourceOperationLedger();

        ledger.Append(
            entry);

        Assert.Equal(
            1,
            ledger.Count);

        Assert.Same(
            entry,
            ledger.Entries[0]);
    }

    [Fact]
    public void Append_PreservesGlobalOperationOrder()
    {
        var loss =
            CreateLossEntry(
                amount: 10d);

        var cost =
            CreateCostEntry(
                amount: 20d);

        var recovery =
            CreateRecoveryEntry(
                amount: 30d);

        var ledger =
            new ResourceOperationLedger();

        ledger.Append(loss);
        ledger.Append(cost);
        ledger.Append(recovery);

        Assert.Equal(
            3,
            ledger.Count);

        Assert.Same(
            loss,
            ledger.Entries[0]);

        Assert.Same(
            cost,
            ledger.Entries[1]);

        Assert.Same(
            recovery,
            ledger.Entries[2]);
    }

    [Fact]
    public void Entries_PreserveConcreteOperationTypes()
    {
        var ledger =
            new ResourceOperationLedger();

        ledger.Append(
            CreateLossEntry(10d));

        ledger.Append(
            CreateCostEntry(20d));

        ledger.Append(
            CreateRecoveryEntry(30d));

        Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[0]);

        Assert.IsType<ResourceCostLedgerEntry>(
            ledger.Entries[1]);

        Assert.IsType<ResourceRecoveryLedgerEntry>(
            ledger.Entries[2]);
    }

    [Fact]
    public void Entries_ExposeCommonResourceIdentityAndProvenance()
    {
        var entry =
            CreateLossEntry(
                amount: 25d);

        ResourceOperationLedgerEntry common =
            entry;

        Assert.Equal(
            entry.Request.ResourceId,
            common.ResourceId);

        Assert.Equal(
            entry.Request.Provenance,
            common.Provenance);
    }

    [Fact]
    public void Append_WithNullEntry_Throws()
    {
        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<ArgumentNullException>(
            () =>
                ledger.Append(
                    null!));
    }

    [Fact]
    public void EntriesCollection_IsReadOnly()
    {
        var ledger =
            new ResourceOperationLedger();

        ledger.Append(
            CreateLossEntry(10d));

        var collection =
            Assert.IsAssignableFrom<
                IList<ResourceOperationLedgerEntry>>(
                ledger.Entries);

        Assert.Throws<NotSupportedException>(
            () =>
                collection.Add(
                    CreateLossEntry(20d)));
    }

    private static ResourceLossLedgerEntry CreateLossEntry(
        double amount)
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
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request);

        return ResourceLossOperations.Commit(
            target,
            preview);
    }

    private static ResourceCostLedgerEntry CreateCostEntry(
        double amount)
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
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                registry,
                target.State,
                request);

        return ResourceCostOperations.Commit(
            target,
            preview);
    }

    private static ResourceRecoveryLedgerEntry CreateRecoveryEntry(
        double amount)
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
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var preview =
            ResourceRecoveryOperations.Preview(
                target.State,
                request);

        return ResourceRecoveryOperations.Commit(
            target,
            preview);
    }
}