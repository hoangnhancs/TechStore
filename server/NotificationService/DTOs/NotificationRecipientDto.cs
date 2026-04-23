using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DTOs
{
    public class NotificationRecipientDto
    {
         public int Id { get; set; }
        public required string NotificationId { get; set; } 
        public required string UserId { get; set; } 
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}