using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NotificationService.DTOs;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class GetNotificationGroupsByUserIdQuery : IRequest<AppResult<List<NotificationGroupDto>>>
    {
        public required string UserId { get; set; }
    }
}