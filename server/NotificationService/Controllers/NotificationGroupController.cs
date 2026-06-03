using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationService.Services.NotificationGroup;
using Shared.Web.Controller;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationGroupController : BaseApiController
    {
        [HttpGet("admin-group")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminNotificationGroup()
        {
            return HandleAppResult(await Mediator.Send(new GetAllAdminNotiGroupQuery()));
        }
    }
}