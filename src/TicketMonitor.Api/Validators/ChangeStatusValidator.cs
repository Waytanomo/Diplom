using FluentValidation;
using TicketMonitor.Core.DTOs;

namespace TicketMonitor.Api.Validators;

public class ChangeStatusValidator : AbstractValidator<ChangeStatusDto>
{
    public ChangeStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Неверный статус заявки");
    }
}