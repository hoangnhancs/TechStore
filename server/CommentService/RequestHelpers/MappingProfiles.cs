using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommentService.DTOs;
using CommentService.Entities;

namespace CommentService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CommentDto, Comment>().ReverseMap();
        }   
    }
}