using System.Collections.ObjectModel;

namespace Idler.Core.Tags;

public sealed class CompiledTagSet
{
    private readonly ReadOnlyCollection<TagId> _directTags;
    private readonly ReadOnlyCollection<TagId> _effectiveTags;

    internal CompiledTagSet(
        TagId[] directTags,
        TagId[] effectiveTags)
    {
        _directTags = Array.AsReadOnly(
            (TagId[])directTags.Clone());

        _effectiveTags = Array.AsReadOnly(
            (TagId[])effectiveTags.Clone());
    }

    public IReadOnlyList<TagId> DirectTags =>
        _directTags;

    public IReadOnlyList<TagId> EffectiveTags =>
        _effectiveTags;

    public bool HasDirect(TagId tag)
    {
        return Contains(_directTags, tag);
    }

    public bool Has(TagId tag)
    {
        return Contains(_effectiveTags, tag);
    }

    public bool MatchesAny(
        IEnumerable<TagId> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        foreach (var tag in tags)
        {
            if (Has(tag))
            {
                return true;
            }
        }

        return false;
    }

    public bool MatchesAll(
        IEnumerable<TagId> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        foreach (var tag in tags)
        {
            if (!Has(tag))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Contains(
        IReadOnlyList<TagId> tags,
        TagId tag)
    {
        foreach (var candidate in tags)
        {
            if (candidate == tag)
            {
                return true;
            }
        }

        return false;
    }
}