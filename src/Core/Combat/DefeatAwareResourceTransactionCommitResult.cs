using Idler.Core.Entities;

namespace Idler.Core.Combat;

public sealed class DefeatAwareResourceTransactionCommitResult
{
    public EntityId EntityId { get; }

    public ProjectedEntityDefeatEvaluation FinalEvaluation { get; }

    public PreDefeatInterventionPhaseResult? PreDefeatPhaseResult { get; }

    public DefeatAwareResourceTransactionCommitOutcome Outcome { get; }

    public bool DefeatWasPrevented =>
        Outcome ==
        DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented;

    public bool DefeatWasAccepted =>
        Outcome ==
        DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted;

    internal DefeatAwareResourceTransactionCommitResult(
        EntityId entityId,
        ProjectedEntityDefeatEvaluation finalEvaluation,
        PreDefeatInterventionPhaseResult? preDefeatPhaseResult,
        DefeatAwareResourceTransactionCommitOutcome outcome)
    {
        if (entityId == default)
        {
            throw new ArgumentException(
                "Entity ID must be valid.",
                nameof(entityId));
        }

        ArgumentNullException.ThrowIfNull(
            finalEvaluation);

        if (finalEvaluation.EntityId !=
            entityId)
        {
            throw new InvalidOperationException(
                "Final defeat evaluation belongs to a different entity.");
        }

        EntityId =
            entityId;

        FinalEvaluation =
            finalEvaluation;

        PreDefeatPhaseResult =
            preDefeatPhaseResult;

        Outcome =
            outcome;
    }
}