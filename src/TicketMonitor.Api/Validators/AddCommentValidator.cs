using FluentValidation;
using TicketMonitor.Core.DTOs;

namespace TicketMonitor.Api.Validators;

public class AddCommentValidator : AbstractValidator<AddCommentDto>
{
    public AddCommentValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Комментарий должен содержать текст")
            .MaximumLength(1000)
            .WithMessage("Слишком длинный комментарий");
    }
}