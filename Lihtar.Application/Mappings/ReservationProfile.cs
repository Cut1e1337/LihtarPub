using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class ReservationProfile : Profile
{
    public ReservationProfile()
    {
        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.TableNumber,
                opt => opt.MapFrom(src => src.Table != null ? src.Table.TableNumber : 0))
            .ForMember(dest => dest.Seats,
                opt => opt.MapFrom(src => src.Table != null ? src.Table.Seats : 0));

        CreateMap<ReservationDto, Reservation>();
    }
}