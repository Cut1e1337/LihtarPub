using FluentValidation;
using Lihtar.Application.DTOs;

namespace Lihtar.Application.Validators;

public class MenuItemDtoValidator : AbstractValidator<MenuItemDto>
{
    public MenuItemDtoValidator()
    {
        RuleFor(x => x.MenuCategoryId)
            .NotEmpty().WithMessage("Оберіть категорію");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова")
            .MaximumLength(120).WithMessage("Назва занадто довга (макс 120)");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис занадто довгий (макс 1000)");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна не може бути відʼємною");

        RuleFor(x => x.Calories)
            .GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue)
            .WithMessage("Калорії не можуть бути відʼємними");

        RuleFor(x => x.WeightGrams)
            .GreaterThan(0).When(x => x.WeightGrams.HasValue)
            .WithMessage("Вага має бути > 0");

        RuleFor(x => x.TagIds)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Теги не повинні повторюватись");
    }
}