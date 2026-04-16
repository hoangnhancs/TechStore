using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Entities
{
    public class NotificationGroupMember
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string NotificationGroupId { get; set; }
        public NotificationGroup? NotificationGroup { get; set; }
        public required string UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}