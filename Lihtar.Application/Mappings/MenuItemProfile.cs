using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class MenuItemProfile : Profile
{
    public MenuItemProfile()
    {
        // Entity -> DTO
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(d => d.TagIds, opt => opt.MapFrom(s => s.TagLinks.Select(x => x.TagId)));

        // DTO -> Entity (простий мап, теги окремо)
        CreateMap<MenuItemDto, MenuItem>()
            .ForMember(d => d.TagLinks, opt => opt.Ignore()) // будемо робити вручну (надійніше)
            .ForMember(d => d.MenuCategory, opt => opt.Ignore())
            .ForMember(d => d.OrderItems, opt => opt.Ignore())
            .ForMember(d => d.MenuItemReviews, opt => opt.Ignore())
            .ForMember(d => d.Favorites, opt => opt.Ignore());
    }
}