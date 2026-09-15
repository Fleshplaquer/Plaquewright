using System.Collections.ObjectModel;
using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Conditions;

public sealed class TagRequirementDefinition
{
    private readonly ReadOnlyCollection<TagKey> _tags;

    public TagRequirementMode Mode { get; }

    public IReadOnlyList<TagKey> Tags => _tags;

    public TagRequirementDefinition(
    TagRequirementMode mode,
    IEnumerable<TagKey> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Unknown tag requirement mode.");
        }

        Mode = mode;

        var copiedTags = tags.ToArray();

        if (copiedTags.Length == 0)
        {
            throw new ArgumentException(
                "A tag requirement must contain at least one tag.",
                nameof(tags));
        }

        _tags = Array.AsReadOnly(copiedTags);
    }
}