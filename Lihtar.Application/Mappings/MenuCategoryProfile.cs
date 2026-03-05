using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class MenuCategoryProfile : Profile
{
    public MenuCategoryProfile()
    {
        CreateMap<MenuCategory, MenuCategoryDto>();

        // DTO -> Entity (Items ігноруємо)
        CreateMap<MenuCategoryDto, MenuCategory>()
            .ForMember(d => d.Items, opt => opt.Ignore());
    }
}