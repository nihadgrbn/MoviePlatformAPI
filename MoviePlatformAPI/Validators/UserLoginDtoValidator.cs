using FluentValidation;
using MoviePlatformAPI.DTOs;

namespace MoviePlatformAPI.Validators;

public class UserLoginDtoValidator:AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Length(3, 20).WithMessage("Password must be between 6 and 20 characters");
    }
    
}