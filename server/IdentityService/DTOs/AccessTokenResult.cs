using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.DTOs
{
    public class AccessTokenResult
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}