using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class ProjectedDefeatCandidateDetectorTests
{
    [Fact]
    public void DefeatRelevantResource_DepletedByProjection_ProducesCandidate()
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
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.NotNull(
            candidate);

        Assert.Same(
            setup.LifeTarget,
            candidate.ResourceTarget);

        Assert.Equal(
            setup.Entity.Id,
            candidate.EntityId);

        Assert.Equal(
            setup.LifeId,
            candidate.ResourceId);

        Assert.Equal(
            100d,
            candidate.OriginalCurrent);

        Assert.Equal(
            0d,
            candidate.ProjectedCurrent);

        Assert.Equal(
            100d,
            candidate.ProjectedMaximum);

        // Detection does not commit.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void DefeatRelevantResource_RemainingAboveZero_ProducesNoCandidate()
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
                amount: 80d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.Null(
            candidate);

        var projected =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            20d,
            projected.Current);
    }

    [Fact]
    public void ResourceAlreadyAtZero_DoesNotProduceNewCandidate()
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
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.Null(
            candidate);

        Assert.Equal(
            0d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void TemporaryProjectedDepletionFollowedByRecovery_ProducesNoFinalCandidate()
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
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var projected =
            draft.GetProjectedValues(
                setup.LifeTarget);

        Assert.Equal(
            25d,
            projected.Current);

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.Null(
            candidate);
    }

    [Fact]
    public void NonDefeatRelevantResource_AtProjectedZero_ProducesNoCandidate()
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
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing)));

        var projected =
            draft.GetProjectedValues(
                setup.ManaTarget);

        Assert.Equal(
            0d,
            projected.Current);

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.ManaTarget);

        Assert.Null(
            candidate);
    }

    [Fact]
    public void UntouchedPositiveDefeatRelevantResource_ProducesNoCandidate()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var candidate =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.Null(
            candidate);

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);
    }

    [Fact]
    public void Detection_DoesNotMutateDraftOrResourceState()
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
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        var operationCountBefore =
            draft.OperationCount;

        _ =
            ProjectedDefeatCandidateDetector.Detect(
                draft,
                setup.LifeTarget);

        Assert.Equal(
            projectionCountBefore,
            draft.ProjectedResourceCount);

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void TargetFromDifferentRegistry_IsRejectedWhenProjectedReadIsRequired()
    {
        var first =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var second =
            CreateSetup(
                currentLife: 100d,
                currentMana: 50d);

        var draft =
            new ResourceTransactionDraft(
                first.Registry);

        Assert.Throws<ArgumentException>(
            () =>
                ProjectedDefeatCandidateDetector.Detect(
                    draft,
                    second.LifeTarget));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);
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
            entity,
            lifeTarget,
            manaTarget);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId ManaId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget ManaTarget);
}