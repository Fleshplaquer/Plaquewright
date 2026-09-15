using System.Collections.ObjectModel;

namespace Plaquewright.Core.Resources;

public sealed class ResourceStateSet
{
    private readonly ResourceState?[] _statesByIndex;
    private readonly ReadOnlyCollection<ResourceState> _states;

    public int Count =>
        _states.Count;

    public IReadOnlyList<ResourceState> States =>
        _states;

    public ResourceStateSet(
        CompiledResourceRegistry registry,
        IEnumerable<ResourceState> states)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(states);

        _statesByIndex =
            new ResourceState?[registry.Count];

        var copiedStates =
            new List<ResourceState>();

        foreach (var sourceState in states)
        {
            if (sourceState is null)
            {
                throw new ArgumentException(
                    "Resource states cannot contain null entries.",
                    nameof(states));
            }

            ValidateKnownId(
                registry,
                sourceState.Id);

            var index =
                sourceState.Id.Value - 1;

            if (_statesByIndex[index] is not null)
            {
                throw new InvalidOperationException(
                    $"Duplicate resource state for ID " +
                    $"'{sourceState.Id}'.");
            }

            var copiedState =
                new ResourceState(
                    sourceState.Id,
                    sourceState.Current,
                    sourceState.Maximum);

            _statesByIndex[index] =
                copiedState;

            copiedStates.Add(
                copiedState);
        }

        copiedStates.Sort(
            static (left, right) =>
                left.Id.CompareTo(right.Id));

        _states =
            copiedStates.AsReadOnly();
    }

    public bool Contains(
        ResourceId id)
    {
        var index =
            ValidateAndGetIndex(id);

        return _statesByIndex[index]
               is not null;
    }

    public ResourceState Get(
        ResourceId id)
    {
        var index =
            ValidateAndGetIndex(id);

        var state =
            _statesByIndex[index];

        if (state is null)
        {
            throw new KeyNotFoundException(
                $"Resource ID '{id}' is not present in this state set.");
        }

        return state;
    }

    private int ValidateAndGetIndex(
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
            (uint)_statesByIndex.Length)
        {
            throw new KeyNotFoundException(
                $"Unknown resource ID '{id}'.");
        }

        return index;
    }

    private static void ValidateKnownId(
        CompiledResourceRegistry registry,
        ResourceId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(id));
        }

        // Validation against the registry.
        _ = registry.GetDefinition(id);
    }
}