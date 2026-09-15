using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationOwnerInvariantTests
{
    [Fact]
    public void LossCommit_WithTargetForDifferentState_DoesNotMutateEitherState()
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

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var preview =
            ResourceLossOperations.Preview(
                firstTarget.State,
                new ResourceLossRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            100d,
            firstTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            100d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }

    [Fact]
    public void RecoveryCommit_WithTargetForDifferentState_DoesNotMutateEitherState()
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

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var preview =
            ResourceRecoveryOperations.Preview(
                firstTarget.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRecoveryOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            50d,
            firstTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            50d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }


    [Fact]
    public void CostCommit_WithTargetForDifferentState_DoesNotMutateEitherState()
    {
        var registry =
            CreateRegistry();

        var manaId =
            GetManaId(
                registry);

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 50d,
                    maximum: 50d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 50d,
                    maximum: 50d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                manaId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                manaId);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                firstTarget.State,
                new ResourceCostRequest(
                    manaId,
                    20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Commit(
                    secondTarget,
                    preview));

        Assert.Equal(
            50d,
            firstTarget.State.Current);

        Assert.Equal(
            0UL,
            firstTarget.State.Revision);

        Assert.Equal(
            50d,
            secondTarget.State.Current);

        Assert.Equal(
            0UL,
            secondTarget.State.Revision);
    }

    [Fact]
    public void SuccessfulLowLevelCommits_PreserveTargetEntityId()
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

        var lossEntity =
            new EntityRuntimeState(
                targetEntityId,
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var lossTarget =
            new ResourceStateTarget(
                lossEntity,
                lifeId);

        var lossPreview =
            ResourceLossOperations.Preview(
                lossTarget.State,
                new ResourceLossRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        var lossEntry =
            ResourceLossOperations.Commit(
                lossTarget,
                lossPreview);

        var recoveryEntity =
            new EntityRuntimeState(
                targetEntityId,
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 50d,
                    maximum: 100d)
                ]);

        var recoveryTarget =
            new ResourceStateTarget(
                recoveryEntity,
                lifeId);

        var recoveryPreview =
            ResourceRecoveryOperations.Preview(
                recoveryTarget.State,
                new ResourceRecoveryRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        var recoveryEntry =
            ResourceRecoveryOperations.Commit(
                recoveryTarget,
                recoveryPreview);

        var costEntity =
            new EntityRuntimeState(
                targetEntityId,
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 50d,
                    maximum: 50d)
                ]);

        var costTarget =
            new ResourceStateTarget(
                costEntity,
                manaId);

        var costPreview =
            ResourceCostOperations.Preview(
                registry,
                costTarget.State,
                new ResourceCostRequest(
                    manaId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        var costEntry =
            ResourceCostOperations.Commit(
                costTarget,
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