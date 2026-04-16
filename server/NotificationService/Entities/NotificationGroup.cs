using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Entities
{
    public class NotificationGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public NotificationGroupType Type { get; set; }
    
        // Chỉ dùng khi Type = ProductFollowers
        public string? ReferenceId { get; set; }       // productId
        public string? ReferenceType { get; set; }     // "Product"
        public List<NotificationGroupMember> Members { get; set; } = [];
    }
    public enum NotificationGroupType
    {
        Admin,
        AllUsers,
        ProductFollowers,
    }
}