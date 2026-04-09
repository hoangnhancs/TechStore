using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class UpdateAddressCommand : IRequest<AppResult<AddressDto>>
    {
        public required string UserId { get; set; }
        public required string AddressId { get; set; }
        public required AddressDto Address { get; set; }
    }
}