using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class PreDefeatRecoveryInterventionResult
{
    public PreDefeatInterventionContext Context { get; }

    public ResourceStateTarget ResourceTarget { get; }

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public double RequestedRecovery { get; }

    public double ProjectedCurrentBefore { get; }

    public double ProjectedCurrentAfter { get; }

    public double ActualProjectedRecovery =>
        ProjectedCurrentAfter -
        ProjectedCurrentBefore;

    public ProjectedEntityDefeatEvaluation EvaluationBefore { get; }

    public ProjectedEntityDefeatEvaluation EvaluationAfter { get; }

    public bool WasDefeatResolved =>
        EvaluationBefore.IsProjectedDefeated &&
        !EvaluationAfter.IsProjectedDefeated;

    public bool IsStillProjectedDefeated =>
        EvaluationAfter.IsProjectedDefeated;

    internal PreDefeatRecoveryInterventionResult(
        PreDefeatInterventionContext context,
        ResourceStateTarget resourceTarget,
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

        if (projectedCurrentAfter <
            projectedCurrentBefore)
        {
            throw new InvalidOperationException(
                "Pre-defeat recovery cannot reduce the projected resource value.");
        }

        Context =
            context;

        ResourceTarget =
            resourceTarget;

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