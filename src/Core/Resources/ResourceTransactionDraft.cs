using System.Collections.ObjectModel;

namespace Plaquewright.Core.Resources;

internal sealed class ResourceTransactionDraft
{
    private readonly CompiledResourceRegistry _registry;

    private readonly Dictionary<
        ResourceState,
        ResourceStateProjection> _projectionsByState =
            new(ReferenceEqualityComparer.Instance);

    private ulong _version;

    private readonly List<ResourceStateProjection>
        _projections = [];

    private readonly List<StagedResourceOperation>
        _operations = [];

    private readonly ReadOnlyCollection<ResourceStateProjection>
        _readOnlyProjections;

    private readonly ReadOnlyCollection<StagedResourceOperation>
        _readOnlyOperations;

    internal ulong Version =>
_version;

    public ResourceTransactionDraft(
        CompiledResourceRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        _registry =
            registry;

        _readOnlyProjections =
            _projections.AsReadOnly();

        _readOnlyOperations =
            _operations.AsReadOnly();
    }

    public int ProjectedResourceCount =>
        _projections.Count;

    public int OperationCount =>
        _operations.Count;

    public IReadOnlyList<ResourceStateProjection> Projections =>
        _readOnlyProjections;

    public IReadOnlyList<StagedResourceOperation> Operations =>
        _readOnlyOperations;

    public ResourceLossPreview StageLoss(
        ResourceStateTarget target,
        ResourceLossRequest request,
        double preventedLoss = 0d)
    {
        ValidateTarget(
            target);
        EnsureVersionCanAdvance();

        var projection =
            GetProjectionCandidate(
                target.State,
                out var isNew);

        var preview =
            projection.PreviewLoss(
                request,
                preventedLoss);

        projection.Apply(
            preview);

        RegisterProjectionIfNew(
            target.State,
            projection,
            isNew);

        _operations.Add(
            new StagedResourceLossOperation(
                target,
                preview));
        AdvanceVersion();
        return preview;
    }

    public ResourceCostPreview StageCost(
        ResourceStateTarget target,
        ResourceCostRequest request)
    {
        ValidateTarget(
            target);
        EnsureVersionCanAdvance();

        var projection =
            GetProjectionCandidate(
                target.State,
                out var isNew);

        var preview =
            projection.PreviewCost(
                request);
        if (!preview.IsPayable)
        {
            throw new InvalidOperationException(
                "An unpayable resource cost cannot be staged.");
        }

        projection.Apply(
            preview);

        RegisterProjectionIfNew(
            target.State,
            projection,
            isNew);

        _operations.Add(
            new StagedResourceCostOperation(
                target,
                preview));
        AdvanceVersion();
        return preview;
    }

    public ResourceRecoveryPreview StageRecovery(
        ResourceStateTarget target,
        ResourceRecoveryRequest request)
    {
        ValidateTarget(
            target);
        EnsureVersionCanAdvance();

        var projection =
            GetProjectionCandidate(
                target.State,
                out var isNew);

        var preview =
            projection.PreviewRecovery(
                request);

        projection.Apply(
            preview);

        RegisterProjectionIfNew(
            target.State,
            projection,
            isNew);

        _operations.Add(
            new StagedResourceRecoveryOperation(
                target,
                preview));
        AdvanceVersion();
        return preview;
    }

    internal ProjectedResourceValues GetProjectedValues(
    ResourceStateTarget target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        if (!ReferenceEquals(
                target.ResourceRegistry,
                _registry))
        {
            throw new ArgumentException(
                "Resource target belongs to a different resource registry.",
                nameof(target));
        }

        foreach (var projection in Projections)
        {
            if (!ReferenceEquals(
                    projection.OriginalState,
                    target.State))
            {
                continue;
            }

            return new ProjectedResourceValues(
                target.ResourceId,
                projection.Current,
                projection.Maximum,
                isProjected: true);
        }

        return new ProjectedResourceValues(
            target.ResourceId,
            target.State.Current,
            target.State.Maximum,
            isProjected: false);
    }

    internal CompiledResourceRegistry ResourceRegistry =>
    _registry;

    private void ValidateTarget(
        ResourceStateTarget target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        if (!ReferenceEquals(
                target.ResourceRegistry,
                _registry))
        {
            throw new ArgumentException(
                "Resource target belongs to a different resource registry.",
                nameof(target));
        }
    }

    private ResourceStateProjection GetProjectionCandidate(
        ResourceState state,
        out bool isNew)
    {
        if (_projectionsByState.TryGetValue(
                state,
                out var existing))
        {
            isNew = false;

            return existing;
        }

        isNew = true;

        return new ResourceStateProjection(
            _registry,
            state);
    }

    private void RegisterProjectionIfNew(
        ResourceState state,
        ResourceStateProjection projection,
        bool isNew)
    {
        if (!isNew)
        {
            return;
        }

        _projectionsByState.Add(
            state,
            projection);

        _projections.Add(
            projection);
    }
    private void EnsureVersionCanAdvance()
    {
        if (_version ==
            ulong.MaxValue)
        {
            throw new InvalidOperationException(
                "Resource transaction draft version is exhausted.");
        }
    }

    private void AdvanceVersion()
    {
        _version++;
    }
}