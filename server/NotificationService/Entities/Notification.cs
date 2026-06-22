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

        // Phân loại cao
        [Column(TypeName = "varchar(20)")]
        public NotificationCategory Category { get; set; }  // System | Order | Payment | Interaction | Promotion
        [Column(TypeName = "varchar(30)")]
        public NotificationType Type { get; set; }           // chi tiết hơn

        // Reference đến entity liên quan
        public string? ReferenceId { get; set; } 
        [Column(TypeName = "varchar(20)")]    // orderId, commentId, reviewId...
        public NotificationReferenceType? ReferenceType { get; set; }   // Order, Comment, Review
        public string? ParentReferenceId { get; set; }
        [Column(TypeName = "varchar(20)")]    // orderId, commentId, reviewId...
        public NotificationReferenceType? ParentReferenceType => ReferenceType;   // Order, Comment, Review //same as ReferenceType, but used for replies (e.g. comment reply sẽ có ParentReferenceType = Comment)
        public List<NotificationRecipient> Recipients { get; set; } = [];
        // Sender: null nếu là System
        public string? SenderId { get; set; }
        // public string? SenderName { get; set; }
        public Notification() : base(Guid.NewGuid().ToString())
        {
        }
        public void AddRecipient(string userId)
        {
            if (Recipients.Any(r => r.UserId == userId))
                return; // Đã là người nhận, không thêm nữa

            Recipients.Add(new NotificationRecipient
            {
                Notification = this,
                NotificationId = this.Id,
                UserId = userId,
            });
        }
        public enum NotificationReferenceType
        {
            Order,
            Comment,
            Review,
            Product,
            Other
        }
        public enum NotificationCategory
        {
            System,
            Order,
            Payment,
            Interaction, //Tương tác giữa người với người
            Promotion //Khuyến mãi / marketing
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
            OrderCancelled,     // huỷ xong

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