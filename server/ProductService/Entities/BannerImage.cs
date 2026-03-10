using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class BannerImage : BaseEntity<int>
    {
        public required string Url { get; set; }
        public string? Title { get; set; }
        public required string PublicId { get; set; }
    }
}