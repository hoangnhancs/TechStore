using AutoMapper;
using VoucherService.DTOs;
using VoucherService.Entities;

namespace VoucherService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Voucher, VoucherDto>()
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(src => src.Scope.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
