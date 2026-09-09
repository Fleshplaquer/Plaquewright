namespace Idler.Core.Combat;

public enum DefeatAwareResourceTransactionCommitOutcome
{
    NoDefeatTransition = 1,
    DefeatPrevented = 2,
    DefeatAccepted = 3
}