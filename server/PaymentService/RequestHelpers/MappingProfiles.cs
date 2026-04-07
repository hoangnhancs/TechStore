using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PaymentService.DTOs;
using PaymentService.Entities;

namespace PaymentService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()));

            CreateMap<CreatePaymentDto, Payment>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => Enum.Parse<Payment.PaymentMethodType>(src.PaymentMethod)));
        }
    }
}