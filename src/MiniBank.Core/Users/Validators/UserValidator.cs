using System.Data;
using FluentValidation;

namespace MiniBank.Core.Users.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(it => it.Login).NotEmpty().WithMessage("Login должен быть не пустым");
        RuleFor(it => it.Login.Length).LessThanOrEqualTo(20).WithMessage("Login не должен превышать 20 символов");
        RuleFor(it => it.Email).NotEmpty().WithMessage("Email должен быть не пустым");
    }
}