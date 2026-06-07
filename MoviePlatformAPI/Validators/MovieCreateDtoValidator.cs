using FluentValidation;
using MoviePlatformAPI.DTOs.Movies;

namespace MoviePlatformAPI.Validators;

public class MovieCreateDtoValidator : AbstractValidator<MovieCreateDto>
{
    public MovieCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Title must be at most 100 characters long.")
            .Must(title => !ContainsScriptTags(title))
            .WithMessage("HTML/Script tags are not allowed in title.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(1000).WithMessage("Description must be at most 1000 characters long.")
            .Must(desc => !ContainsScriptTags(desc))
            .WithMessage("HTML/Script tags are not allowed in description.");

        RuleFor(x => x.ReleaseYear)
            .NotEmpty().WithMessage("Release year is required.")
            .GreaterThan(1888).WithMessage("Release year must be greater than 1888.")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 5)
            .WithMessage("Release year cannot be more than 5 years in the future.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Invalid genre selected. Please choose one of the available genres.")
            .NotEmpty().WithMessage("Genre is required.");
    }

    private bool ContainsScriptTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var lowerText = text.ToLower();
        return lowerText.Contains("<script") ||
               lowerText.Contains("</script>") ||
               lowerText.Contains("javascript:") ||
               lowerText.Contains("onerror=") ||
               lowerText.Contains("onclick=") ||
               lowerText.Contains("onload=");
    }
}