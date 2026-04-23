using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DTOs
{
    public class CreateNotificationDto : NotificationDto
    {
        public string? ReceiverId { get; set; }
        public string? GroupId { get; set; }
    }
}