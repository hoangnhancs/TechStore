using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace NotificationService.Entities
{
    public class NotificationGroup : BaseEntity<string>
    {
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "varchar(20)")]
        public NotificationGroupType Type { get; set; }
    
        // Chỉ dùng khi Type = Subscriptions
        public string? ReferenceId { get; set; }       // productId, flashSaleId, ...
        public string? ReferenceType { get; set; }     // "Product", FlashSale, ...
        public List<NotificationGroupMember> Members { get; set; } = [];
        public NotificationGroup() : base(Guid.NewGuid().ToString())
        {
        }

        public void AddMember(string userId)
        {
            if (Members.Any(m => m.UserId == userId))
                return; // Đã là thành viên, không thêm nữa

            Members.Add(new NotificationGroupMember
            {
                NotificationGroupId = this.Id,
                UserId = userId,
                // UserName = userName,
                // UserImageUrl = userImageUrl
            });
        }
    }

    public enum NotificationGroupType
    {
        Admins,
        Users,
        Subscriptions,
    }
}