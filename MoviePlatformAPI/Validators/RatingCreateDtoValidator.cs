using FluentValidation;
using MoviePlatformAPI.DTOs.Ratings;

namespace MoviePlatformAPI.Validators;

public class RatingCreateDtoValidator:AbstractValidator<RatingCreateDto>
{
    public RatingCreateDtoValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Rating score must be between 1 and 5");
    }
}