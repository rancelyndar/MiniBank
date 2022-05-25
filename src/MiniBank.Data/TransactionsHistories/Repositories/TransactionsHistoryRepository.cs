using Microsoft.EntityFrameworkCore;
using MiniBank.Core.BankAccounts.Models;

namespace MiniBank.Core.TransactionsHistories.Repositories;

public class TransactionsHistoryRepository : ITransactionsHistoryRepository
{
    private readonly MiniBankContext _context;

    public TransactionsHistoryRepository(MiniBankContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken token)
    {
        return await _context.Transactions
            .Select(transaction => new Transaction()
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId
        })
            .ToListAsync(token);
    }

    public async Task CreateTransactionAsync(TransactionCreateModel transaction, CancellationToken token)
    {
        var newTransaction = new TransactionsHistoryDbModel()
        {
            Id = Guid.NewGuid().ToString(),
            Amount = transaction.Amount,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId
        };
            
        await _context.Transactions.AddAsync(newTransaction, token);
        _context.SaveChanges();
    }
}