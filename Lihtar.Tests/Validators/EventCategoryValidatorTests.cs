using Lihtar.Application.DTOs;
using Lihtar.Application.Validators;

namespace Lihtar.Tests.Validators;

public class EventCategoryValidatorTests
{
    private readonly EventCategoryDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new EventCategoryDto
        {
            Name = ""
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        var dto = new EventCategoryDto
        {
            Name = "Концерт",
            Description = new string('a', 501)
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_When_Data_Is_Correct()
    {
        var dto = new EventCategoryDto
        {
            Name = "Концерт",
            Description = "Живий виступ"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}