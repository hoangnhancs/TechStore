using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DTOs
{
    public class NotificationGroupMemberDto
    {
        public string? Id { get; set; }
        public string? NotificationGroupId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}