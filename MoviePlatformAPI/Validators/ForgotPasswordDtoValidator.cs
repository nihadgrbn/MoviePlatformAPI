using FluentValidation;
using MoviePlatformAPI.DTOs.Auth;

namespace MoviePlatformAPI.Validators;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email has to be filled.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}