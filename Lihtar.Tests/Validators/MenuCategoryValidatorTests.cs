using Lihtar.Application.DTOs;
using Lihtar.Application.Validators;

namespace Lihtar.Tests.Validators;

public class MenuCategoryValidatorTests
{
    private readonly MenuCategoryDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new MenuCategoryDto
        {
            Name = "",
            SortOrder = 0
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_SortOrder_Is_Negative()
    {
        var dto = new MenuCategoryDto
        {
            Name = "Напої",
            SortOrder = -1
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_When_Data_Is_Correct()
    {
        var dto = new MenuCategoryDto
        {
            Name = "Напої",
            SortOrder = 1,
            IsActive = true
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}