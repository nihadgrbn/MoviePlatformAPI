using FluentValidation;
using FluentValidation.Validators;
using MoviePlatformAPI.DTOs;

namespace MoviePlatformAPI.Validators;

public class MovieCreateDtoValidator:AbstractValidator<MovieCreateDto>
{
    public MovieCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters long")
            .MaximumLength(50).WithMessage("Title must be at most 50 characters long");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(2).WithMessage("Description must be at least 2 characters long")
            .MaximumLength(500).WithMessage("Description must be at most 500 characters long");
        RuleFor(x => x.ReleaseYear)
            .NotEmpty().WithMessage("Release year is required")
            .GreaterThan(1888).WithMessage("Release year must be greater than 1888");
        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre is required");
        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre must be a valid genre");
    }
    
}