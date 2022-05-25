using FluentValidation;
using MiniBank.Core.BankAccounts.Models;

namespace MiniBank.Core.BankAccounts.Validators;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountModel>
{
    public UpdateAccountValidator()
    {
        RuleFor(it => it.IsOpen).Equal(true).WithMessage("Аккаунт закрыт");
        RuleFor(it => it.Amount).GreaterThanOrEqualTo(0).WithMessage("Недостаточно средств на аккаунте");
    }
}