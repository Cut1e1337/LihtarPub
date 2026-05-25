using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(
                dest => dest.TableNumber,
                opt => opt.MapFrom(src =>
                    src.Table != null
                        ? (int?)src.Table.TableNumber
                        : null
                )
            );

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(
                dest => dest.MenuItemName,
                opt => opt.MapFrom(src =>
                    src.MenuItem != null
                        ? src.MenuItem.Name
                        : ""
                )
            )
            .ForMember(
                dest => dest.TotalPrice,
                opt => opt.MapFrom(src =>
                    src.Quantity * src.Price
                )
            );
    }
}