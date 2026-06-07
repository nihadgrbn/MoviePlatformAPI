using FluentValidation;
using MoviePlatformAPI.DTOs.Auth;

namespace MoviePlatformAPI.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code cannot be empty.")
            .Length(6).WithMessage("OTP code must be exactly 6 characters long.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password cannot be empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character (e.g., !@#$%^&*)!");
    }
}