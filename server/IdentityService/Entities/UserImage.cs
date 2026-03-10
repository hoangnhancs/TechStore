using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace IdentityService.Entities
{
    public class UserImage : BaseEntity<string>
    {
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        public required string UserId { get; set; }
        public User? User { get; set; }

        public UserImage() : base(Guid.NewGuid().ToString())
        {
        }
    }
}