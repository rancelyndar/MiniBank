using FluentValidation;

namespace MiniBank.Core.BankAccounts.Validators;

public class CloseAccountValidator : AbstractValidator<BankAccount>
{
    public CloseAccountValidator()
    {
        RuleFor(it => it.IsOpen).Equal(true).WithMessage("Аккаунт уже закрыт");
        RuleFor(it => it.Amount).Equal(0).WithMessage("Невозможно закрыть аккаунт с ненулевым балансом, " +
                                                      "для закрытия переведите средства на другой аккаунт");
    }
}