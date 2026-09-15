namespace Plaquewright.Core.Combat;

public sealed class PreDefeatInterventionPhaseResult
{
    public PreDefeatInterventionContext Context { get; }

    public ProjectedEntityDefeatEvaluation FinalEvaluation { get; }

    public PreDefeatInterventionPhaseOutcome Outcome { get; }

    public bool WasResolved =>
        Outcome ==
        PreDefeatInterventionPhaseOutcome.Resolved;

    public bool RemainsProjectedDefeated =>
        FinalEvaluation.IsProjectedDefeated;

    internal ulong DraftVersion { get; }

    internal PreDefeatInterventionPhaseResult(
        PreDefeatInterventionContext context,
        ProjectedEntityDefeatEvaluation finalEvaluation,
        PreDefeatInterventionPhaseOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            finalEvaluation);

        if (finalEvaluation.EntityId !=
            context.EntityId)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase final evaluation belongs to a different entity.");
        }

        if (finalEvaluation.Policy !=
            context.Policy)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase final evaluation uses a different defeat policy.");
        }

        var expectedOutcome =
            finalEvaluation.IsProjectedDefeated
                ? PreDefeatInterventionPhaseOutcome.Unresolved
                : PreDefeatInterventionPhaseOutcome.Resolved;

        if (outcome !=
            expectedOutcome)
        {
            throw new InvalidOperationException(
                "Pre-defeat phase outcome does not match the final projected defeat state.");
        }

        Context =
            context;

        FinalEvaluation =
            finalEvaluation;

        Outcome =
            outcome;

        DraftVersion =
context.Draft.Version;
    }
}