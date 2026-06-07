using FluentValidation;
using MoviePlatformAPI.DTOs.Movies;

namespace MoviePlatformAPI.Validators;

public class MovieQueryParametersDtoValidator : AbstractValidator<MovieQueryParametersDto>
{
    public MovieQueryParametersDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters.")
            .Must(term => !ContainsDangerousCharacters(term))
            .When(x => !string.IsNullOrEmpty(x.SearchTerm))
            .WithMessage("Search term contains invalid characters.");

        RuleFor(x => x.Genre)
            .IsInEnum().When(x => x.Genre.HasValue)
            .WithMessage("Invalid genre specified.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                           sortBy.Equals("Title", StringComparison.OrdinalIgnoreCase) ||
                           sortBy.Equals("Year", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort by must be either 'Title' or 'Year'.");
    }

    private bool ContainsDangerousCharacters(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var lowerText = text.ToLower();
        return lowerText.Contains("<script") ||
               lowerText.Contains("</script>") ||
               lowerText.Contains("javascript:") ||
               lowerText.Contains("--") ||  
               lowerText.Contains("/*") ||  
               lowerText.Contains("*/") ||
               lowerText.Contains(";--") ||
               lowerText.Contains("'or'") ||
               lowerText.Contains("1=1");
    }
}
