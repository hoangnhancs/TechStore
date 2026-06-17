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
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<NotificationGroup, NotificationGroupDto>();
            CreateMap<NotificationGroupDto, NotificationGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<NotificationGroupMember, NotificationGroupMemberDto>()
                .ReverseMap();
            CreateMap<NotificationRecipient, NotificationRecipientDto>()
                .ReverseMap();
        }
    }
}