using Plaquewright.Core.Entities;
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
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount: 30d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Sacrifice));

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
            ResourceOperationCause.Sacrifice,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            30d,
            entry.Result.ActualLoss);

        Assert.Equal(
            70d,
            state.Current);
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

        var state =
            new ResourceState(
                registry.GetId(key),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceCostRequest(
                state.Id,
                amount: 25d,
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
            ResourceOperationCause.SkillCost,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            25d,
            entry.Result.ActualCost);

        Assert.Equal(
            75d,
            state.Current);
    }

    [Fact]
    public void RecoveryCommit_DoesNotLoseAccessToOriginalProvenance()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        var request =
            new ResourceRecoveryRequest(
                state.Id,
                amount: 20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Regeneration));

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
            ResourceOperationCause.Regeneration,
            preview.Request.Provenance.Cause);

        Assert.Equal(
            20d,
            entry.Result.ActualRecovery);

        Assert.Equal(
            70d,
            state.Current);
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
}