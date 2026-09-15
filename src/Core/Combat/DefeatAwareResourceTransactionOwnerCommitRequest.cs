using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Combat;

internal sealed class DefeatAwareResourceTransactionOwnerCommitRequest
{
    public EntityRuntimeState Entity { get; }

    public DefeatRelevantResourcePolicy Policy { get; }

    public PreDefeatInterventionPhaseResult?
        PreDefeatPhaseResult
    { get; }

    public DefeatAwareResourceTransactionOwnerCommitRequest(
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        PreDefeatInterventionPhaseResult? preDefeatPhaseResult = null)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        Entity =
            entity;

        Policy =
            policy;

        PreDefeatPhaseResult =
            preDefeatPhaseResult;
    }
}