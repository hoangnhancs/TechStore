using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace IdentityService.Entities
{
    public class Address : BaseEntity<string>
    {
        public required string UserId { get; set; }
        public User? User { get; set; }
        public required string FullName { get; set; }
        public required string Province { get; set; }
        public required string ProvinceCode { get; set; }
        public string? District { get; set; }
        public string? DistrictCode { get; set; }
        public required string Ward { get; set; }
        public required string WardCode { get; set; }
        public required string DetailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDefault { get; set; }
        // Bỏ dòng: public string Id { get; set; } = ...
        public Address() : base(Guid.NewGuid().ToString())
        {
        }
    }
}