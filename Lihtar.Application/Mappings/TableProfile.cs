using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Table, TableDto>().ReverseMap();
    }
}