using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResourceLossResultTests
{
    [Fact]
    public void Constructor_ProducesTypedActualResourceLoss()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                plan.Request,
                preventedLoss: 10d);

        var result =
            new DamageResourceLossResult(
                plan,
                preview);

        Assert.Same(
            plan,
            result.Plan);

        Assert.Equal(
            70d,
            result.DamageTaken.Amount);

        Assert.Equal(
            40d,
            result.RequestedResourceLoss);

        Assert.Equal(
            10d,
            result.PreventedResourceLoss);

        Assert.Equal(
            30d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            0d,
            result.Shortfall);

        Assert.Equal(
            setup.Entity.Id,
            result.TargetEntityId);

        Assert.Equal(
            setup.Context.ResourceTarget.ResourceId,
            result.ResourceId);
    }

    [Fact]
    public void ActualResourceLoss_ComesFromResourceResult_NotDamageTaken()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 90d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 25d);

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                plan.Request);

        var result =
            new DamageResourceLossResult(
                plan,
                preview);

        Assert.Equal(
            90d,
            result.DamageTaken.Amount);

        Assert.Equal(
            25d,
            result.RequestedResourceLoss);

        Assert.Equal(
            25d,
            result.ActualResourceLoss.Amount);
    }

    [Fact]
    public void Result_PreservesPreventionAndShortfallSeparately()
    {
        var setup =
            CreateSetup(
                current: 20d,
                maximum: 100d,
                damageTakenAmount: 100d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 100d);

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                plan.Request,
                preventedLoss: 30d);

        var result =
            new DamageResourceLossResult(
                plan,
                preview);

        Assert.Equal(
            100d,
            result.RequestedResourceLoss);

        Assert.Equal(
            30d,
            result.PreventedResourceLoss);

        Assert.Equal(
            20d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            50d,
            result.Shortfall);

        Assert.Equal(
            result.RequestedResourceLoss,
            result.PreventedResourceLoss +
            result.ActualResourceLoss.Amount +
            result.Shortfall);
    }

    [Fact]
    public void ZeroActualResourceLoss_IsAllowed()
    {
        var setup =
            CreateSetup(
                current: 0d,
                maximum: 100d,
                damageTakenAmount: 50d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 50d);

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                plan.Request);

        var result =
            new DamageResourceLossResult(
                plan,
                preview);

        Assert.Equal(
            0d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            50d,
            result.Shortfall);
    }

    [Fact]
    public void CreatingResult_DoesNotMutateResourceState()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                plan.Request);

        var result =
            new DamageResourceLossResult(
                plan,
                preview);

        Assert.Equal(
            40d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void PreviewFromDifferentRequest_IsRejected()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var otherRequest =
            new ResourceLossRequest(
                plan.ResourceId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var preview =
            ResourceLossOperations.Preview(
                setup.State,
                otherRequest);

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResourceLossResult(
                    plan,
                    preview));

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void PreviewFromDifferentTargetState_IsRejected()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateEntity(
                registry,
                entityId: 2UL,
                current: 100d);

        var secondEntity =
            CreateEntity(
                registry,
                entityId: 3UL,
                current: 100d);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var resolution =
            CreateResolution(
                firstEntity.Id,
                damageTakenAmount: 70d);

        var targetContext =
            new DamageResourceTargetContext(
                resolution,
                firstTarget);

        var plan =
            new DamageResourceLossPlan(
                targetContext,
                requestedResourceLoss: 40d);

        var preview =
            ResourceLossOperations.Preview(
                secondTarget.State,
                plan.Request);

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResourceLossResult(
                    plan,
                    preview));
    }

    private static TestSetup CreateSetup(
        double current,
        double maximum,
        double damageTakenAmount)
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
                        current,
                        maximum)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var resolution =
            CreateResolution(
                entity.Id,
                damageTakenAmount);

        var context =
            new DamageResourceTargetContext(
                resolution,
                resourceTarget);

        return new TestSetup(
            registry,
            entity,
            context,
            resourceTarget.State);
    }

    private static DamageResolutionContext CreateResolution(
        EntityId targetEntityId,
        double damageTakenAmount)
    {
        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(1UL),
                new EntityId(1UL),
                new SimulationTime(100L));

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                targetEntityId,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: damageTakenAmount,
                damageTakenAmount: damageTakenAmount);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

    private static EntityRuntimeState CreateEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double current)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    current,
                    maximum: 100d)
            ]);
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

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        EntityRuntimeState Entity,
        DamageResourceTargetContext Context,
        ResourceState State);
}