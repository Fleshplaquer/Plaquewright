using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceOperationOwnerInvariantTests
{
    [Fact]
    public void LossCommit_WithInvalidTargetEntityId_DoesNotMutateState()
    {
        var lifeId =
            new ResourceId(1);

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        EntityId targetEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                ResourceLossOperations.Commit(
                    targetEntityId,
                    state,
                    preview));

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void RecoveryCommit_WithInvalidTargetEntityId_DoesNotMutateState()
    {
        var lifeId =
            new ResourceId(1);

        var state =
            new ResourceState(
                lifeId,
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                new ResourceRecoveryRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        EntityId targetEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    targetEntityId,
                    state,
                    preview));

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void CostCommit_WithInvalidTargetEntityId_DoesNotMutateState()
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

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                new ResourceCostRequest(
                    manaId,
                    20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        EntityId targetEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                ResourceCostOperations.Commit(
                    targetEntityId,
                    state,
                    preview));

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void SuccessfulLowLevelCommits_PreserveProvidedTargetEntityId()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var manaId =
            GetManaId(
                registry);

        var targetEntityId =
            new EntityId(42UL);

        var lossState =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var lossPreview =
            ResourceLossOperations.Preview(
                lossState,
                new ResourceLossRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        var lossEntry =
            ResourceLossOperations.Commit(
                targetEntityId,
                lossState,
                lossPreview);

        var recoveryState =
            new ResourceState(
                lifeId,
                current: 50d,
                maximum: 100d);

        var recoveryPreview =
            ResourceRecoveryOperations.Preview(
                recoveryState,
                new ResourceRecoveryRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        var recoveryEntry =
            ResourceRecoveryOperations.Commit(
                targetEntityId,
                recoveryState,
                recoveryPreview);

        var costState =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var costPreview =
            ResourceCostOperations.Preview(
                registry,
                costState,
                new ResourceCostRequest(
                    manaId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var costEntry =
            ResourceCostOperations.Commit(
                targetEntityId,
                costState,
                costPreview);

        Assert.Equal(
            targetEntityId,
            lossEntry.TargetEntityId);

        Assert.Equal(
            targetEntityId,
            recoveryEntry.TargetEntityId);

        Assert.Equal(
            targetEntityId,
            costEntry.TargetEntityId);
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