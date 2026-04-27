using FluentValidation;
using MoviePlatformAPI.DTOs.Movies;

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
        RuleFor(m => m.Genre)
            .IsInEnum()
            .WithMessage("Invalid genre selected. Please choose one of the available genres (e.g., Action, Comedy, Drama, etc.).");
        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre must be a valid genre");
    }
    
}