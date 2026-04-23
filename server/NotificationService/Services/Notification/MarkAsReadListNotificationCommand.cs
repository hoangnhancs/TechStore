using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class MarkAsReadListNotificationCommand : IRequest<AppResult<Unit>>
    {
        public required string UserId { get; set; }
        public required List<string> NotificationIds { get; set; }
    }
}