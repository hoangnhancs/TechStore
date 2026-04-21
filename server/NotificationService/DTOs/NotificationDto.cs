using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DTOs
{
    public class NotificationDto
    {
        public string? Id { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;

        // Phân loại cao
        public string? Category { get; set; }  // System | Order | Payment | Interaction | Promotion
        public string? Type { get; set; }           // chi tiết hơn

        // Reference đến entity liên quan
        public string? ReferenceId { get; set; }     // orderId, commentId, reviewId...
        public string? ReferenceType { get; set; }   // "Order", "Comment", "Review"

        // Sender: null nếu là System
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? GroupId { get; set; }
    }
}