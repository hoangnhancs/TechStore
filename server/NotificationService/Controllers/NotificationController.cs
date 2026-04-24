using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services.Notification;
using Shared.Web.Controller;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : BaseApiController
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateNotification([FromBody]CreateNotificationDto dto)
        {
            // Implementation for creating a notification
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            var userName = User.FindFirstValue(ClaimTypes.Name);
            dto.SenderName = userName;
            dto.SenderId = userId;
            return HandleAppResult(await Mediator.Send(new CreateNotificationCommand { CreateNotificationDto = dto }));
        }
    }
}