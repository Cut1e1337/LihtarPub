using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>();

        CreateMap<PaymentItem, PaymentItemDto>()
            .ForMember(dest => dest.MenuItemName,
                opt => opt.MapFrom(src =>
                    src.OrderItem != null && src.OrderItem.MenuItem != null
                        ? src.OrderItem.MenuItem.Name
                        : ""))
            .ForMember(dest => dest.TotalPrice,
                opt => opt.MapFrom(src => src.Quantity * src.Price));
    }
}