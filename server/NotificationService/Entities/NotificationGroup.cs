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
        [Column(TypeName = "nvarchar(20)")]
        public NotificationGroupType Type { get; set; }
    
        // Chỉ dùng khi Type = ProductFollowers
        public string? ReferenceId { get; set; }       // productId
        public string? ReferenceType { get; set; }     // "Product"
        public List<NotificationGroupMember> Members { get; set; } = [];
        public NotificationGroup() : base(Guid.NewGuid().ToString())
        {
        }
    }

    public enum NotificationGroupType
    {
        Admin,
        AllUsers,
        ProductFollowers,
    }
}