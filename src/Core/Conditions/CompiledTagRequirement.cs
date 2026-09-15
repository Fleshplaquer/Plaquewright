using System.Collections.ObjectModel;
using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Conditions;

public sealed class CompiledTagRequirement
{
    private readonly ReadOnlyCollection<TagId> _tags;
    private readonly CompiledTagRegistryIdentity
    _registryIdentity;

    public TagRequirementMode Mode { get; }

    public IReadOnlyList<TagId> Tags => _tags;

    internal CompiledTagRequirement(
    CompiledTagRegistryIdentity registryIdentity,
    TagRequirementMode mode,
    TagId[] tags)
    {
        ArgumentNullException.ThrowIfNull(
    registryIdentity);

        _registryIdentity =
            registryIdentity;
        Mode = mode;

        _tags = Array.AsReadOnly(
            (TagId[])tags.Clone());
    }

    public bool Matches(
    CompiledTagSet tagSet)
    {
        ArgumentNullException.ThrowIfNull(
            tagSet);

        if (!ReferenceEquals(
                _registryIdentity,
                tagSet.RegistryIdentity))
        {
            throw new InvalidOperationException(
                "Compiled tag requirement and tag set belong to different compiled tag registries.");
        }

        return Mode switch
        {
            TagRequirementMode.All =>
                tagSet.MatchesAll(_tags),

            TagRequirementMode.Any =>
                tagSet.MatchesAny(_tags),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(Mode),
                    Mode,
                    "Unknown tag requirement mode.")
        };
    }
}