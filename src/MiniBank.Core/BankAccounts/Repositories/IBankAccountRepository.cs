
using MiniBank.Core.BankAccounts.Models;

namespace MiniBank.Core.BankAccounts.Repositories;

public interface IBankAccountRepository
{
    Task<BankAccount> GetAccountByIdAsync(string id, CancellationToken token);
    Task<IEnumerable<BankAccount>> GetAllAccountsAsync(CancellationToken token);
    Task<IEnumerable<BankAccount>> GetUserAccountsAsync(string userId, CancellationToken token);
    Task CreateAccountAsync(CreateAccountModel account, CancellationToken token);
    Task UpdateAmountOnAccountAsync(UpdateAccountModel account, CancellationToken token);
    Task CloseAccountAsync(string id, CancellationToken token);
    Task<bool> AccountExistsAsync(string id, CancellationToken token);
}