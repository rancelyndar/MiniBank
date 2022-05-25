using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.BankAccounts;
using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.BankAccounts.Services;
using MiniBank.Core.TransactionsHistories;
using MiniBank.Web.Controllers.BankAccounts.Dto;

namespace MiniBank.Web.Controllers.BankAccounts;

[ApiController]
[Authorize]
[Route("[controller]")]
public class BankAccountController
{
    private readonly IBankAccountService _bankAccountService;

    public BankAccountController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    private BankAccountDto ConvertToBankAccountDto(BankAccount account)
    {
        return new BankAccountDto()
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

    [HttpGet("{id}")]
    public async Task<BankAccountDto> GetAccountById(string id, CancellationToken token)
    {
        var account = await _bankAccountService.GetAccountByIdAsync(id, token);

        return ConvertToBankAccountDto(account);
    }
    
    
    [HttpGet("/useraccounts")]
    public async Task<IEnumerable<BankAccountDto>> GetAccounts(CancellationToken token, string? userId = null)
    {
        if (userId == null)
        {
            return (await _bankAccountService.GetAllAccountsAsync(token)).Select(ConvertToBankAccountDto);
        }

        return (await _bankAccountService.GetUserAccountsAsync(userId, token)).Select(ConvertToBankAccountDto);
    }
        

    [HttpPost]
    public Task Create(CreateAccountDto account, CancellationToken token)
    {
        return _bankAccountService.CreateAccountAsync(new CreateAccountModel
        {
            Amount = account.Amount,
            Currency = account.Currency,
            UserId = account.UserId
        }, token);
    } 

    
    [HttpPost("{id}")]
    public Task CloseAccount(string id, CancellationToken token)
    {
        return _bankAccountService.CloseAccountAsync(id, token);
    }


    [HttpGet("/commission")]
    public Task<decimal> CalculateCommission(decimal amount, string fromAccountId, string toAccountId, CancellationToken token)
    {
        return _bankAccountService.CalculateCommissionAsync(amount, fromAccountId, toAccountId, token);
    }

    [HttpPut("/transaction")]
    public Task MakeTransaction(TransactionDto transaction, CancellationToken token)
    {
        return _bankAccountService.MakeTransactionAsync(new TransactionCreateModel
        {
            Amount = transaction.Amount,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId
        }, token);
    }
}