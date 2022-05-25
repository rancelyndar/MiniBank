using FluentValidation;
using MiniBank.Core.BankAccounts.Models;
using MiniBank.Core.TransactionsHistories;

namespace MiniBank.Core.BankAccounts.Validators;

public class TransactionValidator : AbstractValidator<TransactionCreateModel>
{
    public TransactionValidator()
    {
        RuleFor(it => it.Amount).GreaterThan(0).WithMessage("Сумма не может быть меньше 0");
        RuleFor(it => it.FromAccountId)
            .NotEqual(it => it.ToAccountId)
            .WithMessage("Получатель и отправитель должны быть разными");
    }
}