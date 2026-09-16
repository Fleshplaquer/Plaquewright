using Plaquewright.Core.Combat;
using Plaquewright.Core.Tests.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResolutionContextTests
{
    [Fact]
    public void Constructor_PreservesExecutionTargetAndQuantities()
    {
        var execution =
            new DamageExecutionContext(
                new DamageExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new SimulationTime(500L));

        var target =
            new DamageTargetContext(
                execution.Id,
                new EntityId(40UL),
                new HitExecutionId(50UL));

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var context =
            new DamageResolutionContext(
                execution,
                target,
                quantities);

        Assert.Same(
            execution,
            context.Execution);

        Assert.Same(
            target,
            context.Target);

        Assert.Same(
            quantities,
            context.Quantities);

        Assert.Equal(
            new DamageExecutionId(10UL),
            context.DamageExecutionId);

        Assert.Equal(
            new ExecutionId(20UL),
            context.GameplayExecutionId);

        Assert.Equal(
            new EntityId(30UL),
            context.SourceEntityId);

        Assert.Equal(
            new EntityId(40UL),
            context.TargetEntityId);

        Assert.Equal(
            new HitExecutionId(50UL),
            context.RelatedHitExecutionId);

        Assert.True(
            context.IsHitBased);

        Assert.Equal(
            new SimulationTime(500L),
            context.StartedAt);
    }

    [Fact]
    public void Constructor_AllowsNonHitDamage()
    {
        var execution =
            new DamageExecutionContext(
                new DamageExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new SimulationTime(500L));

        var target =
            new DamageTargetContext(
                execution.Id,
                new EntityId(40UL),
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 100d,
                postTakenScalingAmount: 100d,
                damageTakenAmount: 100d);

        var context =
            new DamageResolutionContext(
                execution,
                target,
                quantities);

        Assert.False(
            context.IsHitBased);

        Assert.Null(
            context.RelatedHitExecutionId);
    }

    [Fact]
    public void Constructor_WithDifferentDamageExecutionIds_Throws()
    {
        var execution =
            new DamageExecutionContext(
                new DamageExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new SimulationTime(500L));

        var target =
            new DamageTargetContext(
                new DamageExecutionId(11UL),
                new EntityId(40UL),
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResolutionContext(
                    execution,
                    target,
                    quantities));
    }

    [Fact]
    public void Context_DoesNotDeriveResourceLoss()
    {
        var execution =
            new DamageExecutionContext(
                new DamageExecutionId(10UL),
                new ExecutionId(20UL),
                new EntityId(30UL),
                new SimulationTime(500L));

        var target =
            new DamageTargetContext(
                execution.Id,
                new EntityId(40UL),
                relatedHitExecutionId: null);


        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 80d,
                postTakenScalingAmount: 70d,
                damageTakenAmount: 60d);

        var context =
            new DamageResolutionContext(
                execution,
                target,
                quantities);

        Assert.Equal(
            60d,
            context.Quantities.Taken.Amount);

        // No ActualResourceLoss is part of the
        // damage resolution context. Resource routing
        // remains a later semantic layer.
    }
}