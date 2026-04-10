using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ReviewService.DTOs;
using ReviewService.Entities;

namespace ReviewService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ReviewDto, Review>().ReverseMap();
        }   
    }
}