using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.TransactionsHistories;

namespace MiniBank.Core.BankAccounts.Services;

public interface IBankAccountService
{
    Task<BankAccount> GetAccountByIdAsync(string id, CancellationToken token);
    Task<IEnumerable<BankAccount>> GetAllAccountsAsync(CancellationToken token);
    Task<IEnumerable<BankAccount>> GetUserAccountsAsync(string userId, CancellationToken token);
    Task CreateAccountAsync(CreateAccountModel account, CancellationToken token);
    Task<decimal> CalculateCommissionAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken token);
    Task MakeTransactionAsync(TransactionCreateModel transaction, CancellationToken token);
    Task CloseAccountAsync(string id, CancellationToken token);
}