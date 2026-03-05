using FluentValidation;
using Lihtar.Application.DTOs;

namespace Lihtar.Application.Validators;

public class MenuCategoryDtoValidator : AbstractValidator<MenuCategoryDto>
{
    public MenuCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова")
            .MaximumLength(100).WithMessage("Назва занадто довга (макс 100)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Опис занадто довгий (макс 500)");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("SortOrder не може бути відʼємним");
    }
}