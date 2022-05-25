using Microsoft.EntityFrameworkCore;
using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.BankAccounts.Services;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Core.TransactionsHistories.Repositories;
using MiniBank.Core.Users.Repositories;
using MiniBank.Core.Users.Services;

namespace MiniBank.Core.BankAccounts.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly MiniBankContext _context;

    public BankAccountRepository(MiniBankContext context)
    {
        _context = context;
    }
    
    public async Task<BankAccount> GetAccountByIdAsync(string id, CancellationToken token)
    {
        var account = await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.Id == id, token);

        return new BankAccount()
        {
            Id = account.Id,
            UserId = account.UserId,
            Amount = account.Amount,
            Currency = account.Currency,
            IsOpen = account.IsOpen,
            OpenDate = account.OpenDate,
            CloseDate = account.CloseDate
        };
    }

    public async Task<IEnumerable<BankAccount>> GetAllAccountsAsync(CancellationToken token)
    {
        return await _context.BankAccounts
            .AsNoTracking()
            .Select(account => new BankAccount()
        {
            Id = account.Id,
            UserId = account.UserId,
            Amount = account.Amount,
            Currency = account.Currency,
            IsOpen = account.IsOpen,
            OpenDate = account.OpenDate,
            CloseDate = account.CloseDate
        })
            .ToListAsync(token);
    }

    public async Task<IEnumerable<BankAccount>> GetUserAccountsAsync(string userId, CancellationToken token)
    {
        return await _context.BankAccounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .Select(account => new BankAccount()
        {
            Id = account.Id,
            UserId = account.UserId,
            Amount = account.Amount,
            Currency = account.Currency,
            IsOpen = account.IsOpen,
            OpenDate = account.OpenDate,
            CloseDate = account.CloseDate
        })
            .ToListAsync(token);
    }

    public async Task CreateAccountAsync(CreateAccountModel account, CancellationToken token)
    {
        var newAccount = new BankAccountDbModel()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = account.UserId,
            Amount = account.Amount,
            Currency = account.Currency,
            IsOpen = true,
            OpenDate = DateTime.UtcNow,
        };
            
        await _context.BankAccounts.AddAsync(newAccount, token);
    }

    public async Task UpdateAmountOnAccountAsync(UpdateAccountModel account, CancellationToken token)
    {
        var accountToUpdate = await _context.BankAccounts.FirstOrDefaultAsync(it => it.Id == account.Id, token);

        accountToUpdate.Amount = account.Amount;
    }

    public async Task CloseAccountAsync(string id, CancellationToken token)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(account => account.Id == id, token);
        
        account.IsOpen = false;
        account.CloseDate = DateTime.UtcNow;
    }

    public async Task<bool> AccountExistsAsync(string id, CancellationToken token)
    {
        return await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.Id == id, token) != null;
    }
}