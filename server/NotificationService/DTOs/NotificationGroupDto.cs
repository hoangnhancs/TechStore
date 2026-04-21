using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DTOs
{
    public class NotificationGroupDto
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public string? Type { get; set; }
    
        // Chỉ dùng khi Type = ProductFollowers
        public string? ReferenceId { get; set; }       // productId
        public string? ReferenceType { get; set; }     // "Product"
    }
}