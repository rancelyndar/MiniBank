namespace MiniBank.Core.TransactionsHistories.Services;

public interface ITransactionsHistoryService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken token);
}