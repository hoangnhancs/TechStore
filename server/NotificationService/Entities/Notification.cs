using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace NotificationService.Entities
{
    public class Notification : BaseEntity<string>
    {
        public required string Title { get; set; }
        public required string Message { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;

        // Phân loại cao
        [Column(TypeName = "varchar(20)")]
        public NotificationCategory Category { get; set; }  // System | Order | Payment | Interaction | Promotion
        [Column(TypeName = "varchar(30)")]
        public NotificationType Type { get; set; }           // chi tiết hơn

        // Reference đến entity liên quan
        public string? ReferenceId { get; set; }     // orderId, commentId, reviewId...
        public string? ReferenceType { get; set; }   // "Order", "Comment", "Review"

        // Sender: null nếu là System
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? GroupId { get; set; }
        public NotificationGroup? Group { get; set; }

        public Notification() : base(Guid.NewGuid().ToString())
        {
        }
        public enum NotificationCategory
        {
            System,
            Order,
            Payment,
            Interaction,
            Promotion
        }
        public enum NotificationType
        {
            // System
            SystemAnnouncement,
            AccountSecurity,

            // Order - User nhận
            OrderPlaced,        // đặt hàng + thanh toán ok
            OrderShipping,      // admin cập nhật đang giao
            OrderDelivered,     // giao xong

            // Order - Admin nhận
            NewOrder,           // có đơn mới
            OrderCancelRequest, // user muốn huỷ (nếu có)
            RefundRequest,      // user muốn hoàn tiền (nếu có)

            // Interaction - User nhận
            CommentReply,
            ReviewReply,

            // Interaction - Admin nhận
            NewReview,
            NewComment,

            // Promotion - User nhận
            VoucherReceived,
            FlashSaleAlert,
            
            Other
        }
    }
}