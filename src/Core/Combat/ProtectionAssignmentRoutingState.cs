namespace Plaquewright.Core.Combat;

using Plaquewright.Core.Resources;

public sealed class ProtectionAssignmentRoutingState
{
    private readonly ProtectionAssignmentLaneState[] _lanes;
    private readonly ProtectionAssignmentRoutingLifetime
    _lifetime;

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
     HitExecutionId? relatedHitExecutionId,
     ProtectionAssignmentRoutingLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(
            assignmentResolution);

        ArgumentNullException.ThrowIfNull(
            lanes);

        ArgumentNullException.ThrowIfNull(
lifetime);

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
        _lifetime =
lifetime;

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

        _lifetime.ValidateCurrent(
            this);

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

        var updatedState =
            new ProtectionAssignmentRoutingState(
                AssignmentResolution,
                lanes,
                RelatedHitExecutionId,
                _lifetime);

        _lifetime.Advance(
            this,
            updatedState);

        return updatedState;
    }

    internal ProtectionAssignmentRoutingState FinalizeUnresolvedToPrimaryPath()
    {
        _lifetime.ValidateCurrent(
            this);

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

        var updatedState =
            new ProtectionAssignmentRoutingState(
                AssignmentResolution,
                lanes,
                RelatedHitExecutionId,
                _lifetime);

        _lifetime.Advance(
            this,
            updatedState);

        return updatedState;
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

        var lifetime =
    new ProtectionAssignmentRoutingLifetime();

        var state =
            new ProtectionAssignmentRoutingState(
                assignmentResolution,
                lanes,
                relatedHitExecutionId,
                lifetime);

        lifetime.Initialize(
            state);

        return state;
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

    internal void ValidateCurrentForTransition()
    {
        _lifetime.ValidateCurrent(
            this);
    }

    internal void ValidateResourceTransactionDraft(
        ResourceTransactionDraft draft)
    {
        _lifetime.ValidateResourceTransactionDraft(
            this,
            draft);
    }

    internal void BindResourceTransactionDraft(
        ResourceTransactionDraft draft)
    {
        _lifetime.BindResourceTransactionDraft(
            this,
            draft);
    }
}