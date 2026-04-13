using FluentValidation;
using MoviePlatformAPI.DTOs;

namespace MoviePlatformAPI.Validators;

public class UserRegisterDtoValidator:AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty!")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long!")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters!");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required!")
            .EmailAddress().WithMessage("Please enter a valid email address!");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty!")
            .MinimumLength(6).WithMessage("For security reasons, password must be at least 6 characters long!");
    }
    
}