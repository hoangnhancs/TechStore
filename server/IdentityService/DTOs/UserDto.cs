using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.DTOs
{
    public class UserDto
    {
        public required string Id { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public long TotalSpent { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<string> NotificationGroupIds { get; set; } = [];
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
    }
}