using MiniBank.Core.BankAccounts.Models;

namespace MiniBank.Core.TransactionsHistories.Repositories;

public interface ITransactionsHistoryRepository
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken token);
    Task CreateTransactionAsync(TransactionCreateModel transaction, CancellationToken token);
}