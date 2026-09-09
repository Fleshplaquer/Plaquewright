using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Combat;

public sealed class DamageResourceTargetContextTests
{
    [Fact]
    public void Constructor_BindsResolutionToDamageTargetResource()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var resolution =
            CreateResolution(
                sourceEntityId: new EntityId(1UL),
                targetEntityId: entity.Id);

        var context =
            new DamageResourceTargetContext(
                resolution,
                resourceTarget);

        Assert.Same(
            resolution,
            context.Resolution);

        Assert.Same(
            resourceTarget,
            context.ResourceTarget);
    }

    [Fact]
    public void Constructor_WithResourceOwnedByDifferentEntity_Throws()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var otherEntity =
            new EntityRuntimeState(
                new EntityId(3UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                otherEntity,
                lifeId);

        var resolution =
            CreateResolution(
                sourceEntityId: new EntityId(1UL),
                targetEntityId: new EntityId(2UL));

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResourceTargetContext(
                    resolution,
                    resourceTarget));
    }

    [Fact]
    public void Constructor_WithNonDamageTargetResource_Throws()
    {
        var registry =
            CreateRegistry();

        var manaId =
            GetManaId(
                registry);

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 50d,
                        maximum: 50d)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var resolution =
            CreateResolution(
                sourceEntityId: new EntityId(1UL),
                targetEntityId: entity.Id);

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResourceTargetContext(
                    resolution,
                    resourceTarget));
    }

    private static DamageResolutionContext CreateResolution(
        EntityId sourceEntityId,
        EntityId targetEntityId)
    {
        var execution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(1UL),
                sourceEntityId,
                new SimulationTime(100L));

        var target =
            new DamageTargetContext(
                execution.Id,
                targetEntityId,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                damageTakenAmount: 70d);

        return new DamageResolutionContext(
            execution,
            target,
            quantities);
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