using FluentValidation;
using MoviePlatformAPI.DTOs.Comments;

namespace MoviePlatformAPI.Validators;

public class CommentCreateDtoValidator:AbstractValidator<CommentCreateDto>
{
    public CommentCreateDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty")
            .MaximumLength(500).WithMessage("Comment text cannot be longer than 500 characters");
    }
}