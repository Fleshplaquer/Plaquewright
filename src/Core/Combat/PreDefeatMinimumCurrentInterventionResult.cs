using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class PreDefeatMinimumCurrentInterventionResult
{
    public PreDefeatInterventionContext Context { get; }

    public ResourceStateTarget ResourceTarget { get; }

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public double MinimumCurrent { get; }

    public double RequestedRecovery { get; }

    public double ProjectedCurrentBefore { get; }

    public double ProjectedCurrentAfter { get; }

    public double ActualProjectedRecovery =>
        ProjectedCurrentAfter -
        ProjectedCurrentBefore;

    public bool DidStageRecovery =>
        RequestedRecovery > 0d;

    public ProjectedEntityDefeatEvaluation EvaluationBefore { get; }

    public ProjectedEntityDefeatEvaluation EvaluationAfter { get; }

    public bool WasDefeatResolved =>
        EvaluationBefore.IsProjectedDefeated &&
        !EvaluationAfter.IsProjectedDefeated;

    public bool IsStillProjectedDefeated =>
        EvaluationAfter.IsProjectedDefeated;

    internal PreDefeatMinimumCurrentInterventionResult(
        PreDefeatInterventionContext context,
        ResourceStateTarget resourceTarget,
        double minimumCurrent,
        double requestedRecovery,
        double projectedCurrentBefore,
        double projectedCurrentAfter,
        ProjectedEntityDefeatEvaluation evaluationBefore,
        ProjectedEntityDefeatEvaluation evaluationAfter)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        ArgumentNullException.ThrowIfNull(
            evaluationBefore);

        ArgumentNullException.ThrowIfNull(
            evaluationAfter);

        if (!double.IsFinite(
                minimumCurrent) ||
            minimumCurrent < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumCurrent),
                minimumCurrent,
                "Minimum current must be finite and non-negative.");
        }

        if (!double.IsFinite(
                requestedRecovery) ||
            requestedRecovery < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedRecovery),
                requestedRecovery,
                "Requested recovery must be finite and non-negative.");
        }

        if (projectedCurrentAfter <
            projectedCurrentBefore)
        {
            throw new InvalidOperationException(
                "Pre-defeat minimum-current intervention cannot reduce the projected resource value.");
        }

        Context =
            context;

        ResourceTarget =
            resourceTarget;

        MinimumCurrent =
            minimumCurrent;

        RequestedRecovery =
            requestedRecovery;

        ProjectedCurrentBefore =
            projectedCurrentBefore;

        ProjectedCurrentAfter =
            projectedCurrentAfter;

        EvaluationBefore =
            evaluationBefore;

        EvaluationAfter =
            evaluationAfter;
    }
}