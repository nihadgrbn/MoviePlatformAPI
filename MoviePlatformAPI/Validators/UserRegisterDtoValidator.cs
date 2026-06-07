using FluentValidation;
using MoviePlatformAPI.DTOs.Auth;

namespace MoviePlatformAPI.Validators;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    private static readonly string[] DisposableEmailDomains = new[]
    {
        "tempmail.com", "10minutemail.com", "guerrillamail.com", "mailinator.com",
        "throwaway.email", "temp-mail.org", "getnada.com", "trashmail.com"
    };

    private static readonly string[] CommonWeakPasswords = new[]
    {
        "password", "12345678", "qwerty", "admin123", "welcome1",
        "password1", "letmein", "trustno1", "password123"
    };

    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Username cannot exceed 30 characters.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores.")
            .Must(username => !username.All(char.IsDigit))
            .WithMessage("Username cannot consist only of numbers.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(100).WithMessage("Email address cannot exceed 100 characters.")
            .Must(email => !IsDisposableEmail(email))
            .WithMessage("Disposable email addresses are not allowed.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character (e.g., !@#$%^&*).")
            .Must(password => !IsCommonPassword(password))
            .WithMessage("This password is too common. Please choose a stronger password.");
    }

    private bool IsDisposableEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var domain = email.Split('@').LastOrDefault()?.ToLower();
        return domain != null && DisposableEmailDomains.Contains(domain);
    }

    private bool IsCommonPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return CommonWeakPasswords.Any(weak => password.ToLower().Contains(weak));
    }
}