using Lihtar.Application.DTOs;
using Lihtar.Application.Validators;

namespace Lihtar.Tests.Validators;

public class MenuItemValidatorTests
{
    private readonly MenuItemDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "",
            Price = 100
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Negative()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = -100
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Category_Is_Empty()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.Empty,
            Name = "Burger",
            Price = 100
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Calories_Is_Negative()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = 100,
            Calories = -1
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Weight_Is_Zero()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = 100,
            WeightGrams = 0
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Tags_Are_Duplicated()
    {
        var tagId = Guid.NewGuid();

        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = 100,
            TagIds = new List<Guid> { tagId, tagId }
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_When_Data_Is_Correct()
    {
        var dto = new MenuItemDto
        {
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = 200,
            Calories = 350,
            WeightGrams = 250,
            Description = "Classic burger"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}

