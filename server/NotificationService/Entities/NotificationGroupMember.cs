using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace NotificationService.Entities
{
    public class NotificationGroupMember : BaseEntity<string>
    {
        public required string NotificationGroupId { get; set; }
        public NotificationGroup? NotificationGroup { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public NotificationGroupMember() : base(Guid.NewGuid().ToString())
        {
        }
    }
}