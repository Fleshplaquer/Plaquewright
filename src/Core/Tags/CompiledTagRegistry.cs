using System.Collections.ObjectModel;
using Idler.Core.Conditions;

namespace Idler.Core.Tags;

public sealed class CompiledTagRegistry
{
    private readonly IReadOnlyDictionary<TagKey, TagId> _idsByKey;

    private readonly TagKey[] _keysById;

    private readonly ReadOnlyCollection<TagId>[]
        _directImplications;

    private readonly ReadOnlyCollection<TagId>[]
        _effectiveImplications;

    internal CompiledTagRegistry(
        Dictionary<TagKey, TagId> idsByKey,
        TagKey[] keysById,
        TagId[][] directImplications,
        TagId[][] effectiveImplications)
    {
        _idsByKey =
            new ReadOnlyDictionary<TagKey, TagId>(
                new Dictionary<TagKey, TagId>(idsByKey));

        _keysById =
            (TagKey[])keysById.Clone();

        _directImplications =
            ToReadOnlyCollections(directImplications);

        _effectiveImplications =
            ToReadOnlyCollections(effectiveImplications);
    }

    public int Count => _keysById.Length;

    public TagId GetId(TagKey key)
    {
        if (!_idsByKey.TryGetValue(key, out var id))
        {
            throw new KeyNotFoundException(
                $"Unknown tag key '{key}'.");
        }

        return id;
    }

    public TagKey GetKey(TagId id)
    {
        ValidateId(id);

        return _keysById[ToIndex(id)];
    }

    public IReadOnlyList<TagId> GetDirectImplications(
        TagId id)
    {
        ValidateId(id);

        return _directImplications[ToIndex(id)];
    }

    public IReadOnlyList<TagId> GetEffectiveImplications(
        TagId id)
    {
        ValidateId(id);

        return _effectiveImplications[ToIndex(id)];
    }

    public CompiledTagSet CompileTagSet(
        IEnumerable<TagKey> directKeys)
    {
        ArgumentNullException.ThrowIfNull(directKeys);

        var directTags = new HashSet<TagId>();

        foreach (var key in directKeys)
        {
            directTags.Add(GetId(key));
        }

        var effectiveTags =
            new HashSet<TagId>(directTags);

        foreach (var directTag in directTags)
        {
            foreach (var impliedTag
                     in GetEffectiveImplications(directTag))
            {
                effectiveTags.Add(impliedTag);
            }
        }

        return new CompiledTagSet(
            directTags
                .OrderBy(tag => tag.Value)
                .ToArray(),
            effectiveTags
                .OrderBy(tag => tag.Value)
                .ToArray());
    }

    public CompiledTagRequirement CompileRequirement(
    TagRequirementDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var ids = definition.Tags
            .Select(GetId)
            .Distinct()
            .OrderBy(id => id.Value)
            .ToArray();

        return new CompiledTagRequirement(
            definition.Mode,
            ids);
    }

    private void ValidateId(TagId id)
    {
        if (!id.IsValid ||
            id.Value > _keysById.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                id.Value,
                "TagId does not exist in this registry.");
        }
    }

    private static int ToIndex(TagId id)
    {
        return id.Value - 1;
    }

    private static ReadOnlyCollection<TagId>[]
        ToReadOnlyCollections(TagId[][] source)
    {
        var result =
            new ReadOnlyCollection<TagId>[source.Length];

        for (var index = 0;
             index < source.Length;
             index++)
        {
            var copy =
                (TagId[])source[index].Clone();

            result[index] =
                Array.AsReadOnly(copy);
        }

        return result;
    }
}