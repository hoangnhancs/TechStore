using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shared.Core.EF.Domain.Entities;

namespace IdentityService.Entities
{
    public class User : IdentityUser, IBaseEntity<string>
    {
        public string? DisplayName { get; set; }
        public UserImage? Image { get; set; }
        // public required string BasketId { get; set; } = Guid.NewGuid().ToString();
        public long TotalSpent { get; set; }
        public bool IsAdmin { get; set; } = false;
        public UserGender Gender { get; set; } = UserGender.None;
        public DateOnly? DateOfBirth { get; set; }
        public List<Address> Addresses { get; set; } = [];
        public List<RefreshToken> RefreshTokens { get; set; } = [];
        // public List<string> NotificationGroups { get; set; } = [];
        public bool IsDeleted { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            SetUpdatedAt();
        }

        public void SetUpdatedAt(DateTime? updatedAt = null, string? updatedBy = null)
        {
            UpdatedAt = updatedAt ?? DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
    public enum UserGender
    {
        None,
        Male,
        Female,
        Other,
    }
}