using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation;
using MiniBank.Core.BankAccounts;
using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.BankAccounts.Repositories;
using MiniBank.Core.BankAccounts.Services;
using MiniBank.Core.BankAccounts.Validators;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Core.TransactionsHistories;
using MiniBank.Core.TransactionsHistories.Repositories;
using MiniBank.Core.Users.Repositories;

namespace MiniBank.Core.Tests;
using Moq;
using Xunit;

public class BankAccountServiceTests
{
    private readonly IBankAccountService _bankAccountService;
    private readonly Mock<IBankAccountRepository> _fakeBankAccountRepository;
    private readonly Mock<ICurrencyConverterService> _fakeCurrencyConverterService;
    private readonly Mock<ITransactionsHistoryRepository> _fakeTransactionsHistoryRepository;
    private readonly Mock<IUserRepository> _fakeUserRepository;
    private readonly Mock<IUnitOfWork> _fakeUnitOfWork;
    private readonly IValidator<TransactionCreateModel> _transactionValidator;
    private readonly IValidator<CreateAccountModel> _createAccountValidator;
    private readonly IValidator<BankAccount> _closeAccountValidator;
    private readonly IValidator<UpdateAccountModel> _updateAccountValidator;
    private readonly decimal CommissionRate;

    public BankAccountServiceTests()
    {
        _fakeBankAccountRepository = new Mock<IBankAccountRepository>();
        _fakeCurrencyConverterService = new Mock<ICurrencyConverterService>();
        _fakeTransactionsHistoryRepository = new Mock<ITransactionsHistoryRepository>();
        _fakeUserRepository = new Mock<IUserRepository>();
        _fakeUnitOfWork = new Mock<IUnitOfWork>();
        _transactionValidator = new TransactionValidator();
        _createAccountValidator = new CreateAccountValidator();
        _closeAccountValidator = new CloseAccountValidator();
        _updateAccountValidator = new UpdateAccountValidator();
        CommissionRate = 0.02m;

        _bankAccountService = new BankAccountService(
            _fakeBankAccountRepository.Object,
            _fakeTransactionsHistoryRepository.Object,
            _fakeUserRepository.Object,
            _fakeCurrencyConverterService.Object,
            _fakeUnitOfWork.Object,
            _transactionValidator,
            _createAccountValidator,
            _closeAccountValidator,
            _updateAccountValidator);
    }

    [Fact]
    public void GetAccountByIdAsync_AccountDoesNotExist_ShouldThrowException()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        string fakeId = "Id";

        var exception = Assert.
            ThrowsAsync<ValidationException>(() => _bankAccountService.GetAccountByIdAsync(fakeId, CancellationToken.None));
        
        Assert.Equal($"Аккаунта с  id={fakeId} не существует", exception.Result.Message);
    }

    [Fact]
    public void GetAccountByIdAsync_SuccessPath_ShouldReturnAccount()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new BankAccount());
        string fakeId = "Id";

        var account = _bankAccountService.GetAccountByIdAsync(fakeId, CancellationToken.None);
        
        Assert.IsType<BankAccount>(account.Result);
    }

    [Fact]
    public void GetAllAccountsAsync_SuccessPath_ShouldReturnListOfAccounts()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAllAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<BankAccount>());

        var accounts = _bankAccountService.GetAllAccountsAsync(CancellationToken.None);
        
        Assert.IsType<List<BankAccount>>(accounts.Result);
    }

    [Fact]
    public void GetUserAccountsAsync_UserDoesNotExist_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        var fakeId = "Id";

        var exception = Assert
            .ThrowsAsync<ValidationException>(() =>
                _bankAccountService.GetUserAccountsAsync(fakeId, CancellationToken.None));
        
        Assert.Equal($"Пользователя с id={fakeId} не существует", exception.Result.Message);
    }

    [Fact]
    public void GetUserAccountsAsync_SuccessPath_ShouldReturnListOfAccounts()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetUserAccountsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new List<BankAccount>());
        var fakeId = "Id";

        var accounts = _bankAccountService.GetUserAccountsAsync(fakeId, CancellationToken.None);
        
        Assert.IsType<List<BankAccount>>(accounts.Result);
    }

    [Fact]
    public void CreateAccountAsync_NegativeAmount_ShouldThrowException()
    {
        var account = new CreateAccountModel() {Amount = int.MinValue};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.CreateAccountAsync(account, CancellationToken.None));
        
        Assert.Equal("Сумма не может быть меньше 0",
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void CreateAccountAsync_WrongCurrency_ShouldThrowException()
    {
        var account = new CreateAccountModel() {Amount = 100, Currency = (Currency)25};
        
        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.CreateAccountAsync(account, CancellationToken.None));
        
        Assert.Equal("Недопустимая валюта. Допустимые валюты: RUB, EUR, USD",
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void CreateAccountAsync_UserDoesNotExist_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        var account = new CreateAccountModel() {Amount = 100, Currency = Currency.EUR, UserId = "Id"};

        var exception = Assert.
            ThrowsAsync<ValidationException>(() => _bankAccountService.CreateAccountAsync(account, CancellationToken.None));
        
        Assert.Equal($"Пользователя с id={account.UserId} не существует", exception.Result.Message);
    }

    [Fact]
    public void CreateAccountAsync_SuccessPath_ShouldCallRepositoryMethodOnce()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        var account = new CreateAccountModel() {Amount = 100, Currency = Currency.EUR};

        _bankAccountService.CreateAccountAsync(account, CancellationToken.None);
        
        _fakeBankAccountRepository.Verify(mock => mock.CreateAccountAsync(account, CancellationToken.None), Times.Once);
        _fakeUnitOfWork.Verify(mock => mock.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void CalculateCommissionAsync_WithDifferentUserId_ShouldReturnTwoPerc()
    {
        var toAccountId = "IdTo";
        var fromAccountId = "IdFrom";
        var amount = 100m;
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(toAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {UserId = "IdTo"});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(fromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {UserId = "IdFrom"});

        var commission =
            _bankAccountService.CalculateCommissionAsync(amount, fromAccountId, toAccountId, CancellationToken.None);
        
        Assert.Equal(amount*0.02m, commission.Result);
    }

    [Fact]
    public void CalculateCommissionAsync_WithSameUserId_ShouldReturnZero()
    {
        var toAccountId = "IdTo";
        var fromAccountId = "IdFrom";
        var amount = 100m;
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new BankAccount() {UserId = "Id"});

        var commission =
            _bankAccountService.CalculateCommissionAsync(amount, fromAccountId, toAccountId, CancellationToken.None);

        Assert.Equal(0, commission.Result);
    }

    [Fact]
    public void MakeTransactionAsync_SameAccountsId_ShouldThrowException()
    {
        var transaction = new TransactionCreateModel() {ToAccountId = "Id", FromAccountId = "Id", Amount = 100};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None));
        
        Assert.Equal("Получатель и отправитель должны быть разными", 
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void MakeTransactionAsync_NegativeAmount_ShouldThrowException()
    {
        var transaction = new TransactionCreateModel() {ToAccountId = "IdTo", FromAccountId = "IdFrom", Amount = -100};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None));
        
        Assert.Equal("Сумма не может быть меньше 0", 
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void MakeTransactionAsync_ToAccountIsClosed_ShouldThrowException()
    {
        var transaction = new TransactionCreateModel() {Amount = 100, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = false});

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None));
        
        Assert.Equal("Аккаунт закрыт", 
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void MakeTransactionAsync_FromAccountIsClosed_ShouldThrowException()
    {
        var transaction = new TransactionCreateModel() {Amount = 100, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = false});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100});

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None));
        
        Assert.Equal("Аккаунт закрыт", 
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void MakeTransactionAsync_TransactionAmountIsGreaterThanAccountAmount_ShouldThrowException()
    {
        var transaction = new TransactionCreateModel() {Amount = 1000, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100});

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None));
        
        Assert.Equal("Недостаточно средств на аккаунте", 
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void MakeTransactionAsync_SameCurrencies_ShouldNotCallConvertMethod()
    {
        var transaction = new TransactionCreateModel() {Amount = 100, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.EUR});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.EUR});
        _fakeCurrencyConverterService
            .Setup(service => service
                .ConvertCurrencyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), CancellationToken.None));

        _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None);
        
        _fakeCurrencyConverterService.Verify(mock => mock
            .ConvertCurrencyAsync(Currency.EUR.ToString(), Currency.EUR.ToString(), 100, CancellationToken.None), Times.Never);
    }
    
    [Fact]
    public void MakeTransactionAsync_DifferentCurrencies_ShouldCallConvertMethodOnce()
    {
        var transaction = new TransactionCreateModel() {Amount = 100, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.EUR});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.RUB});
        _fakeCurrencyConverterService
            .Setup(service => service
                .ConvertCurrencyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), CancellationToken.None));

        _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None);
        
        _fakeCurrencyConverterService.Verify(mock => mock
            .ConvertCurrencyAsync(Currency.EUR.ToString(), Currency.RUB.ToString(), 100, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void MakeTransactionAsync_SuccessPath_ShouldCallRepositoryMethods()
    {
        var transaction = new TransactionCreateModel() {Amount = 100, FromAccountId = "IdFrom", ToAccountId = "IdTo"};
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.FromAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.EUR, Id = "IdFrom"});
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(transaction.ToAccountId, CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = true, Amount = 100, Currency = Currency.EUR, Id = "IdTo"});
        _fakeBankAccountRepository
            .Setup(repository =>
                repository.UpdateAmountOnAccountAsync(It.IsAny<UpdateAccountModel>(), CancellationToken.None));
        _fakeCurrencyConverterService
            .Setup(service => service
                .ConvertCurrencyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), CancellationToken.None));
        _fakeTransactionsHistoryRepository
            .Setup(repository => repository.CreateTransactionAsync(It.IsAny<TransactionCreateModel>(), CancellationToken.None));

        _bankAccountService.MakeTransactionAsync(transaction, CancellationToken.None);

        _fakeBankAccountRepository.Verify(
            mock => mock.UpdateAmountOnAccountAsync(It.IsAny<UpdateAccountModel>(), CancellationToken.None),
            Times.Between(2,2, Range.Inclusive));
        _fakeTransactionsHistoryRepository.Verify(
            mock => mock.CreateTransactionAsync(transaction, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void CloseAccountAsync_AccountIsAlreadyClosed_ShouldThrowException()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new BankAccount() {IsOpen = false});
        var fakeId = "Id";

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.CloseAccountAsync(fakeId, CancellationToken.None));
        
        Assert.Equal("Аккаунт уже закрыт",
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void CloseAccountAsync_AccountHasNonZeroAmount_ShouldThrowException()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new BankAccount() {Amount = 100, IsOpen = true});
        var fakeId = "Id";

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _bankAccountService.CloseAccountAsync(fakeId, CancellationToken.None));
        
        Assert.Equal("Невозможно закрыть аккаунт с ненулевым балансом, " +
                     "для закрытия переведите средства на другой аккаунт",
            exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void CloseAccountAsync_SuccessPath_ShouldCallRepositoryMethodOnce()
    {
        _fakeBankAccountRepository
            .Setup(repository => repository.AccountExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeBankAccountRepository
            .Setup(repository => repository.GetAccountByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new BankAccount() {Amount = 0, IsOpen = true});
        var fakeId = "Id";

        _bankAccountService.CloseAccountAsync(fakeId, CancellationToken.None);
        
        _fakeBankAccountRepository
            .Verify(mock => mock.CloseAccountAsync(fakeId, CancellationToken.None), Times.Once);
        _fakeUnitOfWork.Verify(mock => mock.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}