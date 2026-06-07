using FluentValidation;
using MoviePlatformAPI.DTOs.Comments;

namespace MoviePlatformAPI.Validators;

public class CommentUpdateDtoValidator : AbstractValidator<CommentUpdateDto>
{
    public CommentUpdateDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty.")
            .MinimumLength(3).WithMessage("Comment must be at least 3 characters long.")
            .MaximumLength(500).WithMessage("Comment text cannot be longer than 500 characters.")
            .Must(text => !ContainsScriptTags(text))
            .WithMessage("HTML/Script tags are not allowed in comments.");
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
