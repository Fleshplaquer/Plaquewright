using System.Collections.ObjectModel;

namespace Plaquewright.Core.Resources;

internal sealed class ResourceStateSetSnapshot
{
    private readonly ReadOnlyCollection<
        ResourceStateSnapshot> _states;

    public IReadOnlyList<ResourceStateSnapshot> States =>
        _states;

    private ResourceStateSetSnapshot(
        IReadOnlyList<ResourceStateSnapshot> states)
    {
        ArgumentNullException.ThrowIfNull(
            states);

        _states =
            Array.AsReadOnly(
                states.ToArray());
    }

    public static ResourceStateSetSnapshot Capture(
        ResourceStateSet stateSet)
    {
        ArgumentNullException.ThrowIfNull(
            stateSet);

        var states =
            new ResourceStateSnapshot[
                stateSet.States.Count];

        for (var index = 0;
             index < stateSet.States.Count;
             index++)
        {
            states[index] =
                ResourceStateSnapshot.Capture(
                    stateSet.States[index]);
        }

        return new ResourceStateSetSnapshot(
            states);
    }

    public ResourceStateSet Restore(
        CompiledResourceRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        return ResourceStateSet.Restore(
            registry,
            _states);
    }
}