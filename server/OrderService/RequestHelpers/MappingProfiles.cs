using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.DTOs;
using OrderService.Entities;

namespace OrderService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PmtMethod.ToString()));
            
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PmtMethod, opt => opt.Ignore()); // PaymentMethod is managed by domain

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();


            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.PmtMethod, opt => opt.MapFrom(src => src.PaymentMethod));

            // CreateOrderDto mappings
            CreateMap<CreateOrderItemDto, OrderItem>();

            CreateMap<Shipment, ShipmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<ShipmentDto, Shipment>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // Status is managed by

            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
                .ForMember(dest => dest.FromStatus, opt => opt.MapFrom(src => src.FromStatus.ToString()))
                .ForMember(dest => dest.ToStatus, opt => opt.MapFrom(src => src.ToStatus.ToString()));

            CreateMap<OrderStatusHistoryDto, OrderStatusHistory>()
                .ForMember(dest => dest.FromStatus, opt => opt.Ignore()) // Status is managed by domain
                .ForMember(dest => dest.ToStatus, opt => opt.Ignore()); // Status is managed
        }
    }
}