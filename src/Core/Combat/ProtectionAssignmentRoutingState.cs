namespace Plaquewright.Core.Combat;

public sealed class ProtectionAssignmentRoutingState
{
    private readonly ProtectionAssignmentLaneState[] _lanes;

    public ProtectionAssignmentResolution AssignmentResolution { get; }

    public HitExecutionId? RelatedHitExecutionId { get; }

    public bool IsHitBased =>
        RelatedHitExecutionId.HasValue;

    public double InitialDamage =>
        AssignmentResolution.InitialDamage;

    public double BasePrimaryPathDamage =>
        AssignmentResolution.PrimaryPathDamage;

    public IReadOnlyList<ProtectionAssignmentLaneState> Lanes { get; }

    public double PrimaryPathDamage
    {
        get
        {
            var total =
                BasePrimaryPathDamage;

            foreach (var lane in _lanes)
            {
                total +=
                    lane.SpillBackDamage;
            }

            return NormalizeZero(
                total);
        }
    }

    public double ContinueRoutingDamage
    {
        get
        {
            var total =
                0d;

            foreach (var lane in _lanes)
            {
                total +=
                    lane.ContinueRoutingDamage;
            }

            return NormalizeZero(
                total);
        }
    }

    public double FinancedDamage
    {
        get
        {
            var total =
                0d;

            foreach (var lane in _lanes)
            {
                total +=
                    lane.FinancedDamage;
            }

            return NormalizeZero(
                total);
        }
    }

    public double AccountedDamage =>
        PrimaryPathDamage +
        ContinueRoutingDamage +
        FinancedDamage;

    public bool IsComplete
    {
        get
        {
            foreach (var lane in _lanes)
            {
                if (!lane.IsComplete)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private ProtectionAssignmentRoutingState(
        ProtectionAssignmentResolution assignmentResolution,
        ProtectionAssignmentLaneState[] lanes,
        HitExecutionId? relatedHitExecutionId)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        ArgumentNullException.ThrowIfNull(
            lanes);

        if (relatedHitExecutionId.HasValue &&
            relatedHitExecutionId.Value == default)
        {
            throw new ArgumentException(
                "Related hit execution ID must be valid.",
                nameof(relatedHitExecutionId));
        }

        AssignmentResolution =
            assignmentResolution;

        RelatedHitExecutionId =
            relatedHitExecutionId;

        _lanes =
            lanes;

        Lanes =
            Array.AsReadOnly(
                _lanes);
    }

    internal static ProtectionAssignmentRoutingState Start(
        ProtectionAssignmentResolution assignmentResolution)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        return StartCore(
            assignmentResolution,
            relatedHitExecutionId: null);
    }

    internal static ProtectionAssignmentRoutingState Start(
        ProtectionAssignmentResolution assignmentResolution,
        DamageTargetContext damageTarget)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        ArgumentNullException.ThrowIfNull(
            damageTarget);

        if (!damageTarget.IsHitBased ||
            !damageTarget.RelatedHitExecutionId.HasValue)
        {
            throw new InvalidOperationException(
                "Hit-based protection routing requires a hit-based damage target.");
        }

        var hitExecutionId =
            damageTarget.RelatedHitExecutionId.Value;

        if (hitExecutionId == default)
        {
            throw new InvalidOperationException(
                "Hit-based damage target has an invalid hit execution ID.");
        }

        return StartCore(
            assignmentResolution,
            hitExecutionId);
    }

    internal ProtectionAssignmentRoutingState Apply(
        ProtectionAssignmentResult assignment,
        ProtectionShortfallRoutingResult routing)
    {
        ArgumentNullException.ThrowIfNull(
            assignment);

        ArgumentNullException.ThrowIfNull(
            routing);

        var laneIndex =
            FindLaneIndex(
                assignment);

        if (laneIndex < 0)
        {
            throw new InvalidOperationException(
                "Protection assignment does not belong to this routing state.");
        }

        var lanes =
            (ProtectionAssignmentLaneState[])
                _lanes.Clone();

        lanes[laneIndex] =
            lanes[laneIndex].Apply(
                routing);

        return new ProtectionAssignmentRoutingState(
            AssignmentResolution,
            lanes,
            RelatedHitExecutionId);
    }

    internal ProtectionAssignmentRoutingState FinalizeUnresolvedToPrimaryPath()
    {
        if (IsComplete)
        {
            return this;
        }

        var lanes =
            (ProtectionAssignmentLaneState[])
                _lanes.Clone();

        for (var index = 0;
             index < lanes.Length;
             index++)
        {
            lanes[index] =
                lanes[index]
                    .FinalizeUnresolvedToPrimaryPath();
        }

        return new ProtectionAssignmentRoutingState(
            AssignmentResolution,
            lanes,
            RelatedHitExecutionId);
    }

    private static ProtectionAssignmentRoutingState StartCore(
        ProtectionAssignmentResolution assignmentResolution,
        HitExecutionId? relatedHitExecutionId)
    {
        var lanes =
            new ProtectionAssignmentLaneState[
                assignmentResolution.Assignments.Count];

        for (var index = 0;
             index < lanes.Length;
             index++)
        {
            lanes[index] =
                ProtectionAssignmentLaneState.Start(
                    assignmentResolution.Assignments[index]);
        }

        return new ProtectionAssignmentRoutingState(
            assignmentResolution,
            lanes,
            relatedHitExecutionId);
    }

    private int FindLaneIndex(
        ProtectionAssignmentResult assignment)
    {
        for (var index = 0;
             index < _lanes.Length;
             index++)
        {
            if (ReferenceEquals(
                    _lanes[index].Assignment,
                    assignment))
            {
                return index;
            }
        }

        return -1;
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}