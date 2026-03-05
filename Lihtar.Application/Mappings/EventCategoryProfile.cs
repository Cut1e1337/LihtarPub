using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class EventCategoryProfile : Profile
{
    public EventCategoryProfile()
    {
        CreateMap<EventCategory, EventCategoryDto>();

        CreateMap<EventCategoryDto, EventCategory>()
            .ForMember(d => d.Events, opt => opt.Ignore());
    }
}