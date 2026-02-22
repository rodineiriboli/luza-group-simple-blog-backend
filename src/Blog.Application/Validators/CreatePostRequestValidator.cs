using Blog.Application.Posts.Commands;
using FluentValidation;

namespace Blog.Application.Validators;

public sealed class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Content)
            .NotEmpty();
    }
}
