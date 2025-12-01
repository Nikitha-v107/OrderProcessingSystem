using AutoMapper;
using Order.Domain.DTOs;
using Order.Domain.Models;

namespace Order.Api.MappingProfiles
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<CreateOrderDto, Order.Domain.Models.Order>()
                .ForMember(dest => dest.id, opt => opt.Ignore()); // id is generated in service
            CreateMap<Order.Domain.Models.Order, OrderCreatedDto>();
            CreateMap<Order.Domain.Models.Order, OrderDto>();
        }
    }
}
