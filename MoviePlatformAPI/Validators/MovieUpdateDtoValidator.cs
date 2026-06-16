using FluentValidation;
using MoviePlatformAPI.DTOs.Movies;

namespace MoviePlatformAPI.Validators;

public class MovieUpdateDtoValidator : AbstractValidator<MovieUpdateDto>
{
    public MovieUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .Must(title => title.Trim().Length >= 2)
            .WithMessage("Title must be at least 2 characters long.")
            .Must(title => title.Trim().Length <= 100)
            .WithMessage("Title must be at most 100 characters long.")
            .Must(title => !ContainsScriptTags(title))
            .WithMessage("HTML/Script tags are not allowed in title.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .Must(desc => desc.Trim().Length >= 10)
            .WithMessage("Description must be at least 10 characters long.")
            .Must(desc => desc.Trim().Length <= 1000)
            .WithMessage("Description must be at most 1000 characters long.")
            .Must(desc => !ContainsScriptTags(desc))
            .WithMessage("HTML/Script tags are not allowed in description.");

        RuleFor(x => x.ReleaseYear)
            .GreaterThan(1888)
            .WithMessage("Release year must be greater than 1888.")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 5)
            .WithMessage("Release year cannot be more than 5 years in the future.");

        RuleFor(x => x.Genre)
            .IsInEnum()
            .WithMessage("Invalid genre selected.");
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