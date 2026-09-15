using System.Collections.ObjectModel;

namespace Idler.Core.Tags;

public sealed class TagDefinition
{
    private readonly ReadOnlyCollection<TagKey> _impliedTags;

    public TagKey Key { get; }

    public IReadOnlyList<TagKey> ImpliedTags =>
        _impliedTags;

    public TagDefinition(
        TagKey key,
        IEnumerable<TagKey>? impliedTags = null)
    {
        if (key == default)
        {
            throw new ArgumentException(
                "Tag definition key must be valid.",
                nameof(key));
        }

        var tags =
            impliedTags?.ToArray() ?? [];

        if (tags.Any(
                tag =>
                    tag == default))
        {
            throw new ArgumentException(
                "Implied tag keys must be valid.",
                nameof(impliedTags));
        }

        Key =
            key;

        _impliedTags =
            Array.AsReadOnly(tags);
    }
}