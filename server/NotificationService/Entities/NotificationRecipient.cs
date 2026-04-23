using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace NotificationService.Entities
{
    public class NotificationRecipient : BaseEntity<int>
    {
        public required string NotificationId { get; set; } 
        public Notification? Notification { get; set; } 
        public required string UserId { get; set; } 
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public NotificationRecipient() : base(0)
        {
        }
    }
}