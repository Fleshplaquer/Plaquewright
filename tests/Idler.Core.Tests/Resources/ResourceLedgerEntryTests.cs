using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceLedgerEntryTests
{
    [Fact]
    public void LossCommit_ProducesCommittedEntryWithRequestAndResult()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 40d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        var entry =
            ResourceLossOperations.Commit(
                new EntityId(1UL),
                state,
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
            state.Current);
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

        var state =
            new ResourceState(
                registry.GetId(key),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceCostRequest(
                state.Id,
                amount: 30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                request);

        var entry =
            ResourceCostOperations.Commit(
                new EntityId(1UL),
                state,
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
            state.Current);
    }

    [Fact]
    public void RecoveryCommit_ProducesCommittedEntryWithRequestAndResult()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 40d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Leech));

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                request);

        var entry =
            ResourceRecoveryOperations.Commit(
                new EntityId(1UL),
                state,
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
            state.Current);
    }

    [Fact]
    public void LossLedgerEntry_PreservesGrossAccounting()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 40d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request,
                preventedLoss: 20d);

        var entry =
            ResourceLossOperations.Commit(
                new EntityId(1UL),
                state,
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

        var state =
            new ResourceState(
                registry.GetId(key),
                current: 20d,
                maximum: 100d);

        var request =
            new ResourceCostRequest(
                state.Id,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                request);

        Assert.False(
            preview.IsAffordable);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    new EntityId(1UL),
                    state,
                    preview));

        Assert.Equal(
            20d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }
}