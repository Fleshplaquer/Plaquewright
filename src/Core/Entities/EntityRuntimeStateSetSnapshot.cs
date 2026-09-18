using System.Collections.ObjectModel;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Entities;

internal sealed class EntityRuntimeStateSetSnapshot
{
    private readonly ReadOnlyCollection<
        EntityRuntimeStateSnapshot> _entities;

    public IReadOnlyList<EntityRuntimeStateSnapshot>
        Entities =>
            _entities;

    private EntityRuntimeStateSetSnapshot(
        IReadOnlyList<EntityRuntimeStateSnapshot> entities)
    {
        ArgumentNullException.ThrowIfNull(
            entities);

        _entities =
            Array.AsReadOnly(
                entities.ToArray());
    }

    public static EntityRuntimeStateSetSnapshot Capture(
        EntityRuntimeStateSet stateSet)
    {
        ArgumentNullException.ThrowIfNull(
            stateSet);

        var entities =
            new EntityRuntimeStateSnapshot[
                stateSet.Count];

        var index =
            0;

        foreach (var entity in stateSet.Entities)
        {
            entities[index] =
                EntityRuntimeStateSnapshot.Capture(
                    entity);

            index++;
        }

        return new EntityRuntimeStateSetSnapshot(
            entities);
    }

    public EntityRuntimeStateSet Restore(
        CompiledResourceRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        var restored =
            new EntityRuntimeStateSet(
                registry);

        for (var index = 0;
             index < _entities.Count;
             index++)
        {
            restored.Add(
                _entities[index].Restore(
                    registry));
        }

        return restored;
    }
}