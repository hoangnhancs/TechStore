using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NotificationService.DTOs;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroupMember
{
    public class GetMemberByNotificationGroupQuery : IRequest<AppResult<List<NotificationGroupMemberDto>>>
    {
        public required string GroupId { get; set; }
    }
}