using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NotificationService.DTOs;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroup
{
    public class GetNotificationGroupByIdQuery : IRequest<AppResult<NotificationGroupDto>>
    {
        public required string GroupId { get; set; }
    }
}