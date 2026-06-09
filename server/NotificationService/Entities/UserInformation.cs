using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace NotificationService.Entities
{
    public class UserInformation : BaseEntity<int>
    {
        public required string UserId { get; set; }
        public required string DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public UserInformation() : base() { }
    }
}