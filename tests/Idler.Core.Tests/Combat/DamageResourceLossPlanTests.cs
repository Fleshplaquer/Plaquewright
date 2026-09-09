using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Combat;

public sealed class DamageResourceLossPlanTests
{
    [Fact]
    public void Constructor_CreatesDamageDerivedResourceLossRequest()
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                context,
                requestedResourceLoss: 35d);

        Assert.Same(
            context,
            plan.TargetContext);

        Assert.Equal(
            70d,
            plan.DamageTaken.Amount);

        Assert.Equal(
            35d,
            plan.RequestedResourceLoss);

        Assert.Equal(
            context.ResourceTarget.ResourceId,
            plan.ResourceId);

        Assert.Equal(
            context.Resolution.TargetEntityId,
            plan.TargetEntityId);

        Assert.Equal(
            context.ResourceTarget.ResourceId,
            plan.Request.ResourceId);

        Assert.Equal(
            35d,
            plan.Request.Amount);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            plan.Request.Provenance.Cause);
    }

    [Fact]
    public void RequestedResourceLoss_IsNotDerivedImplicitlyFromDamageTaken()
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 100d);

        var plan =
            new DamageResourceLossPlan(
                context,
                requestedResourceLoss: 25d);

        Assert.Equal(
            100d,
            plan.DamageTaken.Amount);

        Assert.Equal(
            25d,
            plan.RequestedResourceLoss);

        Assert.NotEqual(
            plan.DamageTaken.Amount,
            plan.RequestedResourceLoss);
    }

    [Fact]
    public void RequestedResourceLoss_MayExceedDamageTakenAtPrimitiveConversionBoundary()
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 10d);

        var plan =
            new DamageResourceLossPlan(
                context,
                requestedResourceLoss: 20d);

        Assert.Equal(
            10d,
            plan.DamageTaken.Amount);

        Assert.Equal(
            20d,
            plan.RequestedResourceLoss);
    }

    [Fact]
    public void ZeroRequestedResourceLoss_IsAllowed()
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 50d);

        var plan =
            new DamageResourceLossPlan(
                context,
                requestedResourceLoss: 0d);

        Assert.Equal(
            0d,
            plan.RequestedResourceLoss);

        Assert.Equal(
            0d,
            plan.Request.Amount);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidRequestedResourceLoss_Throws(
        double amount)
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 50d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new DamageResourceLossPlan(
                    context,
                    amount));
    }

    [Fact]
    public void CreatingPlan_DoesNotMutateResourceState()
    {
        var context =
            CreateTargetContext(
                damageTakenAmount: 70d);

        var state =
            context.ResourceTarget.State;

        var plan =
            new DamageResourceLossPlan(
                context,
                requestedResourceLoss: 40d);

        Assert.Equal(
            40d,
            plan.Request.Amount);

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    private static DamageResourceTargetContext CreateTargetContext(
        double damageTakenAmount)
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var targetEntity =
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
                targetEntity,
                lifeId);

        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(1UL),
                new EntityId(1UL),
                new SimulationTime(100L));

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                targetEntity.Id,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                damageTakenAmount: damageTakenAmount);

        var resolution =
            new DamageResolutionContext(
                damageExecution,
                damageTarget,
                quantities);

        return new DamageResourceTargetContext(
            resolution,
            resourceTarget);
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