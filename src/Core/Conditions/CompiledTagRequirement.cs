using System.Collections.ObjectModel;
using Idler.Core.Tags;

namespace Idler.Core.Conditions;

public sealed class CompiledTagRequirement
{
    private readonly ReadOnlyCollection<TagId> _tags;

    public TagRequirementMode Mode { get; }

    public IReadOnlyList<TagId> Tags => _tags;

    internal CompiledTagRequirement(
        TagRequirementMode mode,
        TagId[] tags)
    {
        Mode = mode;

        _tags = Array.AsReadOnly(
            (TagId[])tags.Clone());
    }

    public bool Matches(CompiledTagSet tagSet)
    {
        ArgumentNullException.ThrowIfNull(tagSet);

        return Mode switch
        {
            TagRequirementMode.All =>
                tagSet.MatchesAll(_tags),

            TagRequirementMode.Any =>
                tagSet.MatchesAny(_tags),

            _ => throw new ArgumentOutOfRangeException(
                nameof(Mode),
                Mode,
                "Unknown tag requirement mode.")
        };
    }
}