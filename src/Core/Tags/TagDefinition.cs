using System.Collections.ObjectModel;

namespace Idler.Core.Tags;

public sealed class TagDefinition
{
    private readonly ReadOnlyCollection<TagKey> _impliedTags;

    public TagKey Key { get; }

    public IReadOnlyList<TagKey> ImpliedTags => _impliedTags;

    public TagDefinition(
        TagKey key,
        IEnumerable<TagKey>? impliedTags = null)
    {
        Key = key;

        var tags = impliedTags?.ToArray() ?? [];

        _impliedTags = Array.AsReadOnly(tags);
    }
}