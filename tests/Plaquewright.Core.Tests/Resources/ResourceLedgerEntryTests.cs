using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceLedgerEntryTests
{
    [Fact]
    public void LossCommit_ProducesCommittedEntryWithRequestAndResult()
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
                amount: 40d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceLossOperations.Commit(
                target,
                preview);

        Assert.Equal(
            request,
            entry.Request);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            entry.Provenance.Cause);

        Assert.Equal(
            40d,
            entry.Result.ActualLoss);

        Assert.Equal(
            60d,
            target.State.Current);
    }

    [Fact]
    public void UnaffordableCost_ProducesNoLedgerEntryBecauseCommitFails()
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
                    current: 20d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var request =
            new ResourceCostRequest(
                manaId,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                registry,
                target.State,
                request);

        Assert.False(
            preview.IsAffordable);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    target,
                    preview));

        Assert.Equal(
            20d,
            target.State.Current);

        Assert.Equal(
            0UL,
            target.State.Revision);
    }

    [Fact]
    public void CostCommit_ProducesCommittedEntryWithRequestAndResult()
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
                amount: 30d,
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
            request,
            entry.Request);

        Assert.Equal(
            ResourceOperationCause.SkillCost,
            entry.Provenance.Cause);

        Assert.Equal(
            30d,
            entry.Result.ActualCost);

        Assert.Equal(
            70d,
            target.State.Current);
    }

    [Fact]
    public void RecoveryCommit_ProducesCommittedEntryWithRequestAndResult()
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
                amount: 40d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Leech));

        var preview =
            ResourceRecoveryOperations.Preview(
                target.State,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                target,
                preview);

        Assert.Equal(
            request,
            entry.Request);

        Assert.Equal(
            preview.Result,
            entry.Result);

        Assert.Equal(
            ResourceOperationCause.Leech,
            entry.Provenance.Cause);

        Assert.Equal(
            40d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            90d,
            target.State.Current);
    }

    [Fact]
    public void LossLedgerEntry_PreservesGrossAccounting()
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
                    current: 40d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceLossRequest(
                lifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request,
                preventedLoss: 20d);

        var entry =
            ResourceLossOperations.Commit(
                target,
                preview);

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

        Assert.Equal(
            100d,
            entry.Result.PreventedLoss +
            entry.Result.ActualLoss +
            entry.Result.Shortfall);
    }


}