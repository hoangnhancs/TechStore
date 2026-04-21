using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Entities;

namespace NotificationService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Notification, NotificationDto>()
                .ReverseMap();
            CreateMap<NotificationGroup, NotificationGroupDto>()
                .ReverseMap();
            CreateMap<NotificationGroupMember, NotificationGroupMemberDto>()
                .ReverseMap();
        }
    }
}