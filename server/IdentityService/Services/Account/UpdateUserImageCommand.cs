using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Account
{
    public class UpdateUserImageCommand : IRequest<AppResult<object>>
    {
        public required string UserId { get; set; }
        public required IFormFile NewImage { get; set; }
    }
}