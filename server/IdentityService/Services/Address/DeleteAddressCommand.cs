using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class DeleteAddressCommand : IRequest<AppResult<Unit>>
    {
        public required string AddressId { get; set; }
        public required string UserId { get; set; }        
    }
}