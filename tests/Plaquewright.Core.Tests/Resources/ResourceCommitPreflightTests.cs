using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCommitPreflightTests
{
    [Fact]
    public void LossValidateCommit_DoesNotMutateState()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceLossOperations.Preview(
                state,
                new ResourceLossRequest(
                    state.Id,
                    40d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        ResourceLossOperations.ValidateCommit(
            state,
            preview);

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            100d,
            state.Maximum);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void RecoveryValidateCommit_DoesNotMutateState()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 40d,
                maximum: 100d);

        var preview =
            ResourceRecoveryOperations.Preview(
                state,
                new ResourceRecoveryRequest(
                    state.Id,
                    30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        ResourceRecoveryOperations.ValidateCommit(
            state,
            preview);

        Assert.Equal(
            40d,
            state.Current);

        Assert.Equal(
            100d,
            state.Maximum);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void CostValidateCommit_DoesNotMutateState()
    {
        var registry =
            CreateRegistry();

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var state =
            new ResourceState(
                manaId,
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                new ResourceCostRequest(
                    state.Id,
                    30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        ResourceCostOperations.ValidateCommit(
            state,
            preview);

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void CostValidateCommit_UnaffordableCostThrowsWithoutMutation()
    {
        var registry =
            CreateRegistry();

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var state =
            new ResourceState(
                manaId,
                current: 20d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                registry,
                state,
                new ResourceCostRequest(
                    state.Id,
                    50d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.ValidateCommit(
                    state,
                    preview));

        Assert.Equal(
            20d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void ValidateCommit_StalePreviewThrowsWithoutFurtherMutation()
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

        var firstPreview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        var stalePreview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        _ =
            ResourceLossOperations.Commit(
                target,
                firstPreview);

        Assert.Equal(
            90d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceLossOperations.ValidateCommit(
                    target.State,
                    stalePreview));

        Assert.Equal(
            90d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);
    }

    [Fact]
    public void ApplyValidatedCommit_PerformsExactlyOneMutation()
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

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                new ResourceLossRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        ResourceLossOperations.ValidateCommit(
            target.State,
            preview);

        var entry =
            ResourceLossOperations.ApplyValidatedCommit(
                target,
                preview);

        Assert.Equal(
            75d,
            target.State.Current);

        Assert.Equal(
            1UL,
            target.State.Revision);

        Assert.Equal(
            25d,
            entry.Result.ActualLoss);
    }





    private static CompiledResourceRegistry
        CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource)
        ]);
    }
}