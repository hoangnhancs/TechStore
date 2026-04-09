using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class AddAddressCommand : IRequest<AppResult<AddressDto>>
    {
        public required string UserId { get; set; }
        public AddressDto Address { get; set; } = null!;
    }
}