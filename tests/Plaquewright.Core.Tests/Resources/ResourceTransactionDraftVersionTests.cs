using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionDraftVersionTests
{
    [Fact]
    public void SuccessfulStages_AdvanceVersionExactlyOnceEach()
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

        var draft =
            new ResourceTransactionDraft(
                registry);

        Assert.Equal(
            0UL,
            draft.Version);

        draft.StageLoss(
            lifeTarget,
            new ResourceLossRequest(
                lifeId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct)));

        Assert.Equal(
            1UL,
            draft.Version);

        draft.StageRecovery(
            lifeTarget,
            new ResourceRecoveryRequest(
                lifeId,
                5d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        Assert.Equal(
            2UL,
            draft.Version);

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        Assert.Equal(
            3UL,
            draft.Version);
    }

    [Fact]
    public void RejectedStage_DoesNotAdvanceVersion()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

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
                        manaId,
                        current: 10d,
                        maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        Assert.Throws<InvalidOperationException>(
            () =>
                draft.StageCost(
                    manaTarget,
                    new ResourceCostRequest(
                        manaId,
                        50d,
                        new ResourceOperationProvenance(
                            ResourceOperationCause.SkillCost))));

        Assert.Equal(
            0UL,
            draft.Version);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);
    }
}