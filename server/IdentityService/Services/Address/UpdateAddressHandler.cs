using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class UpdateAddressHandler : IRequestHandler<UpdateAddressCommand, AppResult<AddressDto>>
    {
        public Task<AppResult<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}