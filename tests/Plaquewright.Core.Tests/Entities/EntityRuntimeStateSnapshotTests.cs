using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Entities;

public sealed class EntityRuntimeStateSnapshotTests
{
    [Fact]
    public void EntitySnapshotRestore_PreservesIdResourcesAndRevision()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new EntityRuntimeState(
                new EntityId(7UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var originalLife =
            original.Resources.Get(
                lifeId);

        originalLife.SetValues(
            current: 75d,
            maximum: 100d);

        var snapshot =
            EntityRuntimeStateSnapshot.Capture(
                original);

        originalLife.SetValues(
            current: 50d,
            maximum: 100d);

        var restored =
            snapshot.Restore(
                registry);

        var restoredLife =
            restored.Resources.Get(
                lifeId);

        Assert.Equal(
            new EntityId(7UL),
            restored.Id);

        Assert.Same(
            registry,
            restored.ResourceRegistry);

        Assert.Equal(
            75d,
            restoredLife.Current);

        Assert.Equal(
            100d,
            restoredLife.Maximum);

        Assert.Equal(
            1UL,
            restoredLife.Revision);

        Assert.NotSame(
            original,
            restored);

        Assert.NotSame(
            originalLife,
            restoredLife);

        Assert.Equal(
            50d,
            originalLife.Current);

        Assert.Equal(
            2UL,
            originalLife.Revision);
    }

    [Fact]
    public void EntitySetSnapshotRestore_PreservesEntitiesOrderingAndIndependentState()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var original =
            new EntityRuntimeStateSet(
                registry);

        var entitySeven =
            new EntityRuntimeState(
                new EntityId(7UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 50d,
                        maximum: 50d)
                ]);

        var entityTwo =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        original.Add(
            entitySeven);

        original.Add(
            entityTwo);

        entityTwo.Resources
            .Get(lifeId)
            .SetValues(
                current: 80d,
                maximum: 100d);

        entitySeven.Resources
            .Get(manaId)
            .SetValues(
                current: 30d,
                maximum: 50d);

        var snapshot =
            EntityRuntimeStateSetSnapshot.Capture(
                original);

        //
        // Mutation after capture must not leak into
        // the captured entity graph.
        //
        entityTwo.Resources
            .Get(lifeId)
            .SetValues(
                current: 60d,
                maximum: 100d);

        var restored =
            snapshot.Restore(
                registry);

        Assert.Same(
            registry,
            restored.ResourceRegistry);

        Assert.Equal(
            2,
            restored.Count);

        var restoredIds =
            restored.Entities
                .Select(
                    entity => entity.Id)
                .ToArray();

        Assert.Equal(
            [
                new EntityId(2UL),
                new EntityId(7UL)
            ],
            restoredIds);

        var restoredTwo =
            restored.Get(
                new EntityId(2UL));

        var restoredSeven =
            restored.Get(
                new EntityId(7UL));

        var restoredLife =
            restoredTwo.Resources.Get(
                lifeId);

        var restoredMana =
            restoredSeven.Resources.Get(
                manaId);

        Assert.Equal(
            80d,
            restoredLife.Current);

        Assert.Equal(
            1UL,
            restoredLife.Revision);

        Assert.Equal(
            30d,
            restoredMana.Current);

        Assert.Equal(
            1UL,
            restoredMana.Revision);

        Assert.NotSame(
            entityTwo,
            restoredTwo);

        Assert.NotSame(
            entitySeven,
            restoredSeven);

        //
        // Restored state is a separate continuation.
        //
        restoredLife.SetValues(
            current: 70d,
            maximum: 100d);

        Assert.Equal(
            2UL,
            restoredLife.Revision);

        Assert.Equal(
            60d,
            entityTwo.Resources
                .Get(lifeId)
                .Current);

        Assert.Equal(
            2UL,
            entityTwo.Resources
                .Get(lifeId)
                .Revision);
    }

    private static CompiledResourceRegistry
        CreateRegistry()
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