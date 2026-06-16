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

        // Phân loại cao
        public string? Category { get; set; }  // System | Order | Payment | Interaction (tương tác như review, comment) | Promotion (khuyến mãi)
        public string? Type { get; set; }           // chi tiết hơn

        // Reference đến entity liên quan
        public string? ReferenceId { get; set; }     // orderId, commentId, reviewId...
        public string? ReferenceType { get; set; }   // "Order", "Comment", "Review"
        public string? ParentReferenceId { get; set; }     // orderId, commentId, reviewId...
        public string? ParentReferenceType { get; set; }   // "Order", "
        // Sender: null nếu là System
        public required string SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderImageUrl { get; set; }
    }
}