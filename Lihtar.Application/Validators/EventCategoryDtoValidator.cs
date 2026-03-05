using FluentValidation;
using Lihtar.Application.DTOs;

namespace Lihtar.Application.Validators;

public class EventCategoryDtoValidator : AbstractValidator<EventCategoryDto>
{
    public EventCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова")
            .MaximumLength(100).WithMessage("Назва занадто довга (макс 100)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Опис занадто довгий (макс 500)");
    }
}