using FluentValidation;
using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.BankAccounts.Repositories;
using MiniBank.Core.BankAccounts.Validators;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Core.TransactionsHistories;
using MiniBank.Core.TransactionsHistories.Repositories;
using MiniBank.Core.Users.Repositories;

namespace MiniBank.Core.BankAccounts.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ICurrencyConverterService _currencyConverterService;
    private readonly ITransactionsHistoryRepository _transactionsHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<TransactionCreateModel> _transactionValidator;
    private readonly IValidator<CreateAccountModel> _createAccountValidator;
    private readonly IValidator<BankAccount> _closeAccountValidator;
    private readonly IValidator<UpdateAccountModel> _updateAccountValidator;
    private readonly decimal CommissionRate;

    public BankAccountService(IBankAccountRepository bankAccountRepository,
        ITransactionsHistoryRepository transactionsHistoryRepository,
        IUserRepository userRepository,
        ICurrencyConverterService currencyConverterService,
        IUnitOfWork unitOfWork,
        IValidator<TransactionCreateModel> transactionValidator,
        IValidator<CreateAccountModel> createAccountValidator,
        IValidator<BankAccount> closeAccountValidator,
        IValidator<UpdateAccountModel> updateAccountValidator)
    {
        _bankAccountRepository = bankAccountRepository;
        _userRepository = userRepository;
        _transactionsHistoryRepository = transactionsHistoryRepository;
        _currencyConverterService = currencyConverterService;
        _unitOfWork = unitOfWork;
        _transactionValidator = transactionValidator;
        _createAccountValidator = createAccountValidator;
        _closeAccountValidator = closeAccountValidator;
        _updateAccountValidator = updateAccountValidator;
        CommissionRate = 0.02m;
    }

    private async Task ValidateAndThrow(string id, CancellationToken token)
    {
        if (!await _bankAccountRepository.AccountExistsAsync(id, token))
        {
            throw new ValidationException($"Аккаунта с  id={id} не существует");
        }
    }

    private async Task ValidateAndThrowUser(string userId, CancellationToken token)
    {
        if (!await _userRepository.UserExistsAsync(userId, token))
        {
            throw new ValidationException($"Пользователя с id={userId} не существует");
        }
    }
    
    public async Task<BankAccount> GetAccountByIdAsync(string id, CancellationToken token)
    {
        await ValidateAndThrow(id, token);
        
        return await _bankAccountRepository.GetAccountByIdAsync(id, token);
    }

    public Task<IEnumerable<BankAccount>> GetAllAccountsAsync(CancellationToken token)
    {
        return _bankAccountRepository.GetAllAccountsAsync(token);
    }

    public async Task<IEnumerable<BankAccount>> GetUserAccountsAsync(string userId, CancellationToken token)
    {
        await ValidateAndThrowUser(userId, token);

        return await _bankAccountRepository.GetUserAccountsAsync(userId, token);
    }
    
    public async Task CreateAccountAsync(CreateAccountModel account, CancellationToken token)
    {
        _createAccountValidator.ValidateAndThrow(account);
        await ValidateAndThrowUser(account.UserId, token);

        await _bankAccountRepository.CreateAccountAsync(account, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    private Task<decimal> CalculateCommission(decimal amount, BankAccount from, BankAccount to)
    {
        return Task.FromResult(from.UserId == to.UserId ? 0 : amount * CommissionRate);
    }
    
    public async Task<decimal> CalculateCommissionAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken token)
    {
        var fromAccount = await GetAccountByIdAsync(fromAccountId, token);
        var toAccount = await GetAccountByIdAsync(toAccountId, token);

        return await CalculateCommission(amount, fromAccount, toAccount);
    }

    public async Task MakeTransactionAsync(TransactionCreateModel transaction, CancellationToken token)
    {
        _transactionValidator.ValidateAndThrow(transaction);

        var fromAccount = await GetAccountByIdAsync(transaction.FromAccountId, token);
        var toAccount = await GetAccountByIdAsync(transaction.ToAccountId, token);

        _updateAccountValidator.ValidateAndThrow(new UpdateAccountModel
        {
            Id = fromAccount.Id,
            IsOpen = fromAccount.IsOpen,
            Amount = fromAccount.Amount - transaction.Amount
        });
        _updateAccountValidator.ValidateAndThrow(new UpdateAccountModel
        {
            Id = toAccount.Id,
            IsOpen = toAccount.IsOpen,
            Amount = toAccount.Amount
        });

        await _transactionsHistoryRepository.CreateTransactionAsync(transaction, token);
        
        await _bankAccountRepository.UpdateAmountOnAccountAsync(new UpdateAccountModel
        {
            Amount = fromAccount.Amount - transaction.Amount,
            Id = transaction.FromAccountId
        }, token);
        
        transaction.Amount -= await CalculateCommission(transaction.Amount, fromAccount, toAccount);
        if (fromAccount.Currency != toAccount.Currency)
        {
            transaction.Amount = await _currencyConverterService
                .ConvertCurrencyAsync(fromAccount.Currency.ToString(), toAccount.Currency.ToString(), transaction.Amount, token);
        }

        await _bankAccountRepository.UpdateAmountOnAccountAsync(new UpdateAccountModel
        {
            Amount = toAccount.Amount + transaction.Amount,
            Id = transaction.ToAccountId
        }, token);
        
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task CloseAccountAsync(string id, CancellationToken token)
    {
        var account = await GetAccountByIdAsync(id, token);
        
        _closeAccountValidator.ValidateAndThrow(account);
        
        await _bankAccountRepository.CloseAccountAsync(id, token);
        await _unitOfWork.SaveChangesAsync(token);
    }
}