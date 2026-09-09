using System.Collections.ObjectModel;

namespace Idler.Core.Resources;

public sealed class CompiledResourceRegistry
{
    private readonly ReadOnlyDictionary<
        ResourceKey,
        ResourceId> _idsByKey;

    private readonly CompiledResourceDefinition[]
        _definitionsByIndex;

    internal CompiledResourceRegistry(
        Dictionary<ResourceKey, ResourceId> idsByKey,
        CompiledResourceDefinition[] definitionsByIndex)
    {
        ArgumentNullException.ThrowIfNull(idsByKey);
        ArgumentNullException.ThrowIfNull(definitionsByIndex);

        _idsByKey =
            new ReadOnlyDictionary<ResourceKey, ResourceId>(
                new Dictionary<ResourceKey, ResourceId>(
                    idsByKey));

        _definitionsByIndex =
            (CompiledResourceDefinition[])
            definitionsByIndex.Clone();
    }

    public int Count =>
        _definitionsByIndex.Length;

    public ResourceId GetId(
        ResourceKey key)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException(
                "Resource key must be valid.",
                nameof(key));
        }

        if (!_idsByKey.TryGetValue(
                key,
                out var id))
        {
            throw new KeyNotFoundException(
                $"Unknown resource key '{key}'.");
        }

        return id;
    }

    public ResourceKey GetKey(
        ResourceId id)
    {
        return GetDefinition(id).Key;
    }

    public CompiledResourceDefinition GetDefinition(
        ResourceId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(id));
        }

        var index =
            id.Value - 1;

        if ((uint)index >=
            (uint)_definitionsByIndex.Length)
        {
            throw new KeyNotFoundException(
                $"Unknown resource ID '{id}'.");
        }

        return _definitionsByIndex[index];
    }

    public CompiledResourceDefinition GetDefinition(
        ResourceKey key)
    {
        return GetDefinition(
            GetId(key));
    }
}