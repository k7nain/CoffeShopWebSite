using CoffeeShop.Application.DTOs.Auth;
using FluentValidation;

namespace CoffeeShop.Application.Validators.Auth;

public class RegisterAdminRequestValidator : AbstractValidator<RegisterAdminRequest>
{
    public RegisterAdminRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.SetupSecretKey).NotEmpty();
    }
}
