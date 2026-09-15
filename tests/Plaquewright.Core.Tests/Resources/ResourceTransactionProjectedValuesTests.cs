using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionProjectedValuesTests
{
    [Fact]
    public void UntouchedResource_ReturnsOriginalValues()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var values =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            setup.LifeId,
            values.ResourceId);

        Assert.Equal(
            100d,
            values.Current);

        Assert.Equal(
            100d,
            values.Maximum);

        Assert.False(
            values.IsProjected);

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);
    }

    [Fact]
    public void StagedLoss_ReturnsProjectedValues()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var request =
            new ResourceLossRequest(
                setup.LifeId,
                amount: 70d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        draft.StageLoss(
            setup.LifeTarget,
            request);

        var values =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            30d,
            values.Current);

        Assert.Equal(
            100d,
            values.Maximum);

        Assert.True(
            values.IsProjected);

        // Original still untouched.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void MultipleOperations_ReturnLatestProjectedValues()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 70d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var values =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            50d,
            values.Current);

        Assert.Equal(
            100d,
            values.Maximum);

        Assert.True(
            values.IsProjected);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void ProjectionForOtherResource_DoesNotAffectTargetValues()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.ManaTarget,
            new ResourceLossRequest(
                setup.ManaId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing)));

        var lifeValues =
            draft.GetProjectedValues(
                setup.LifeTarget);

        var manaValues =
            draft.GetProjectedValues(
                setup.ManaTarget);

        Assert.False(
            lifeValues.IsProjected);

        Assert.Equal(
            100d,
            lifeValues.Current);

        Assert.True(
            manaValues.IsProjected);

        Assert.Equal(
            25d,
            manaValues.Current);
    }

    [Fact]
    public void ZeroEffectiveLoss_CanStillHaveProjectedState()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var values =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            0d,
            values.Current);

        Assert.True(
            values.IsProjected);

        Assert.Equal(
            0d,
            setup.LifeTarget.State.Current);
    }

    [Fact]
    public void TargetFromDifferentRegistry_IsRejected()
    {
        var first =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var second =
            CreateSetup(
                currentLife: 80d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                first.Registry);

        Assert.Throws<ArgumentException>(
            () =>
                draft.GetProjectedValues(
                    second.LifeTarget));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);
    }

    [Fact]
    public void ReadingProjectedValues_DoesNotCreateProjection()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        _ =
            draft.GetProjectedValues(
                setup.LifeTarget);

        _ =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    private static TestSetup CreateSetup(
        double currentLife,
        double currentMana)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource |
                    ResourceRole.ProtectionSource)
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
                        currentLife,
                        maximum: 100d),

                    new ResourceState(
                        manaId,
                        currentMana,
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

        return new TestSetup(
            registry,
            lifeId,
            manaId,
            lifeTarget,
            manaTarget);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId ManaId,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget ManaTarget);
}