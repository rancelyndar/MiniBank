using FluentValidation;
using MiniBank.Core.BankAccounts.Models;

namespace MiniBank.Core.BankAccounts.Validators;

public class CreateAccountValidator : AbstractValidator<CreateAccountModel>
{
    public CreateAccountValidator()
    {
        RuleFor(it => it.Amount).GreaterThan(0).WithMessage("Сумма не может быть меньше 0");
        RuleFor(it => it.Currency).IsInEnum().WithMessage("Недопустимая валюта. Допустимые валюты: RUB, EUR, USD");
    }
}