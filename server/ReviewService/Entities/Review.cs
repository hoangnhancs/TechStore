using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ReviewService.Entities
{
    public class Review : BaseEntity<string>
    {
        public required string ProductId { get; set; } = string.Empty;
        public required string UserId { get; set; } = string.Empty;
        public required int Rating { get; set; }
        public string? Content { get; set; } = string.Empty;
        public Review() : base(Guid.NewGuid().ToString())
        {
        }
    }   
}