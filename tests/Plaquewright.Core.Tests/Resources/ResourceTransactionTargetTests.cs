using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionTargetTests
{
    [Fact]
    public void StagedOperation_PreservesTargetEntity()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateEntity(
                registry,
                entityId: 42UL,
                lifeCurrent: 100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            new ResourceLossRequest(
                lifeId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        Assert.Equal(
            new EntityId(42UL),
            operation.EntityId);

        Assert.Equal(
            lifeId,
            operation.ResourceId);

        Assert.Same(
            target,
            operation.Target);

        Assert.Same(
            target.State,
            operation.OriginalState);
    }

    [Fact]
    public void SameResourceIdOnDifferentEntities_RemainsOwnerDistinct()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateEntity(
                registry,
                entityId: 1UL,
                lifeCurrent: 100d);

        var secondEntity =
            CreateEntity(
                registry,
                entityId: 2UL,
                lifeCurrent: 200d);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            firstTarget,
            CreateLossRequest(
                lifeId,
                10d));

        draft.StageLoss(
            secondTarget,
            CreateLossRequest(
                lifeId,
                20d));

        Assert.Equal(
            2,
            draft.OperationCount);

        Assert.Equal(
            firstEntity.Id,
            draft.Operations[0].EntityId);

        Assert.Equal(
            secondEntity.Id,
            draft.Operations[1].EntityId);

        Assert.Equal(
            lifeId,
            draft.Operations[0].ResourceId);

        Assert.Equal(
            lifeId,
            draft.Operations[1].ResourceId);

        Assert.NotSame(
            draft.Operations[0].OriginalState,
            draft.Operations[1].OriginalState);
    }

    [Fact]
    public void TargetFromDifferentRegistry_IsRejectedBeforeStaging()
    {
        var firstRegistry =
            CreateRegistry();

        var secondRegistry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                secondRegistry);

        var entity =
            CreateEntity(
                secondRegistry,
                entityId: 1UL,
                lifeCurrent: 100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                firstRegistry);

        Assert.Throws<ArgumentException>(
            () =>
                draft.StageLoss(
                    target,
                    CreateLossRequest(
                        lifeId,
                        10d)));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            100d,
            target.State.Current);
    }

    private static EntityRuntimeState CreateEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double lifeCurrent)
    {
        var lifeId =
            GetLifeId(
                registry);

        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    lifeId,
                    lifeCurrent,
                    maximum: 200d)
            ]);
    }

    private static ResourceLossRequest CreateLossRequest(
        ResourceId resourceId,
        double amount)
    {
        return new ResourceLossRequest(
            resourceId,
            amount,
            new ResourceOperationProvenance(
                ResourceOperationCause.Direct));
    }

    private static ResourceId GetLifeId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.life"));
    }

    private static CompiledResourceRegistry CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
        ]);
    }
}