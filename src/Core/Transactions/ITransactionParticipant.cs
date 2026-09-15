using System.Diagnostics.CodeAnalysis;

namespace Plaquewright.Core.Transactions;

/// <summary>
/// Participates in a cross-module transaction.
///
/// Preparation must not make gameplay state visible.
/// A participant may reject the transaction by returning false.
/// </summary>
public interface ITransactionParticipant
{
    bool TryPrepare(
        [NotNullWhen(true)]
        out PreparedTransactionChange? preparedChange);
}