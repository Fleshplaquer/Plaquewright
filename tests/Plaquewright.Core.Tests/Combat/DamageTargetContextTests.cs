using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageTargetContextTests
{
    [Fact]
    public void Constructor_WithoutHit_PreservesTargetRelationship()
    {
        var context =
            new DamageTargetContext(
                new DamageExecutionId(10UL),
                new EntityId(20UL),
                relatedHitExecutionId: null);

        Assert.Equal(
            new DamageExecutionId(10UL),
            context.DamageExecutionId);

        Assert.Equal(
            new EntityId(20UL),
            context.TargetEntityId);

        Assert.Null(
            context.RelatedHitExecutionId);

        Assert.False(
            context.IsHitBased);
    }

    [Fact]
    public void Constructor_WithHit_PreservesHitRelationship()
    {
        var context =
            new DamageTargetContext(
                new DamageExecutionId(10UL),
                new EntityId(20UL),
                new HitExecutionId(30UL));

        Assert.Equal(
            new DamageExecutionId(10UL),
            context.DamageExecutionId);

        Assert.Equal(
            new EntityId(20UL),
            context.TargetEntityId);

        Assert.Equal(
            new HitExecutionId(30UL),
            context.RelatedHitExecutionId);

        Assert.True(
            context.IsHitBased);
    }

    [Fact]
    public void Constructor_WithInvalidDamageExecutionId_Throws()
    {
        DamageExecutionId damageExecutionId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageTargetContext(
                    damageExecutionId,
                    new EntityId(20UL),
                    relatedHitExecutionId: null));
    }

    [Fact]
    public void Constructor_WithInvalidTargetEntityId_Throws()
    {
        EntityId targetEntityId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageTargetContext(
                    new DamageExecutionId(10UL),
                    targetEntityId,
                    relatedHitExecutionId: null));
    }

    [Fact]
    public void Constructor_WithInvalidRelatedHitExecutionId_Throws()
    {
        HitExecutionId invalidHitExecutionId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new DamageTargetContext(
                    new DamageExecutionId(10UL),
                    new EntityId(20UL),
                    invalidHitExecutionId));
    }
}