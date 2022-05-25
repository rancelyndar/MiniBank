using MiniBank.Core.TransactionsHistories.Repositories;

namespace MiniBank.Core.TransactionsHistories.Services;

public class TransactionsHistoryService : ITransactionsHistoryService
{
    private readonly ITransactionsHistoryRepository _transactionsHistoryRepository;

    public TransactionsHistoryService(ITransactionsHistoryRepository transactionsHistoryRepository)
    {
        _transactionsHistoryRepository = transactionsHistoryRepository;
    }

    public Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken token)
    {
        return _transactionsHistoryRepository.GetAllTransactionsAsync(token);
    }
}