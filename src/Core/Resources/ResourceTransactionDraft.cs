using System.Collections.ObjectModel;

namespace Idler.Core.Resources;

internal sealed class ResourceTransactionDraft
{
    private readonly CompiledResourceRegistry _registry;

    private readonly Dictionary<
        ResourceState,
        ResourceStateProjection> _projectionsByState =
            new(ReferenceEqualityComparer.Instance);

    private readonly List<ResourceStateProjection>
        _projections = [];

    private readonly List<StagedResourceOperation>
        _operations = [];

    private readonly ReadOnlyCollection<ResourceStateProjection>
        _readOnlyProjections;

    private readonly ReadOnlyCollection<StagedResourceOperation>
        _readOnlyOperations;

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

        return preview;
    }

    public ResourceCostPreview StageCost(
        ResourceStateTarget target,
        ResourceCostRequest request)
    {
        ValidateTarget(
            target);

        var projection =
            GetProjectionCandidate(
                target.State,
                out var isNew);

        var preview =
            projection.PreviewCost(
                request);

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

        return preview;
    }

    public ResourceRecoveryPreview StageRecovery(
        ResourceStateTarget target,
        ResourceRecoveryRequest request)
    {
        ValidateTarget(
            target);

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

        return preview;
    }

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
}