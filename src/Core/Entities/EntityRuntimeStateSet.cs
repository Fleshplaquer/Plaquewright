using Idler.Core.Resources;

namespace Idler.Core.Entities;

public sealed class EntityRuntimeStateSet
{
    private readonly SortedDictionary<
        EntityId,
        EntityRuntimeState> _entities = [];

    public CompiledResourceRegistry ResourceRegistry { get; }

    public int Count =>
        _entities.Count;

    public IReadOnlyCollection<EntityRuntimeState> Entities =>
        _entities.Values;

    public EntityRuntimeStateSet(
        CompiledResourceRegistry resourceRegistry)
    {
        ArgumentNullException.ThrowIfNull(
            resourceRegistry);

        ResourceRegistry =
            resourceRegistry;
    }

    public void Add(
        EntityRuntimeState entity)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        if (!ReferenceEquals(
                entity.ResourceRegistry,
                ResourceRegistry))
        {
            throw new ArgumentException(
                "Entity belongs to a different resource registry.",
                nameof(entity));
        }

        if (!_entities.TryAdd(
                entity.Id,
                entity))
        {
            throw new ArgumentException(
                $"An entity with ID {entity.Id} already exists.",
                nameof(entity));
        }
    }

    public bool Contains(
        EntityId id)
    {
        ValidateId(
            id);

        return _entities.ContainsKey(
            id);
    }

    public EntityRuntimeState Get(
        EntityId id)
    {
        ValidateId(
            id);

        if (!_entities.TryGetValue(
                id,
                out var entity))
        {
            throw new KeyNotFoundException(
                $"Entity {id} does not exist.");
        }

        return entity;
    }

    private static void ValidateId(
        EntityId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Entity ID must be valid.",
                nameof(id));
        }
    }
}