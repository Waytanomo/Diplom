using FluentValidation;
using TicketMonitor.Core.DTOs;

namespace TicketMonitor.Api.Validators;

public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Укажите название")
            .MaximumLength(100)
            .WithMessage("Слишком длинное название");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Укажите описание")
            .MaximumLength(5000)
            .WithMessage("Слишком длинное описание");
    }
}