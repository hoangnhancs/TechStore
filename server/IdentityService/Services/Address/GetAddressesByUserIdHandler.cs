using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityService.DTOs;
using IdentityService.Persistence;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class GetAddressesByUserIdHandler : IRequestHandler<GetAddressesByUserIdQuery, AppResult<List<AddressDto>>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAddressesByUserIdHandler(IIdentityUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }   
        public async Task<AppResult<List<AddressDto>>> Handle(GetAddressesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _unitOfWork.AddressRepository.GetListAsync(
                p => p.UserId == request.UserId,
                cancellationToken: cancellationToken
            );
            if (addresses == null || !addresses.Any())
                return AppResult<List<AddressDto>>.Success(new List<AddressDto>());
            var addressDtos = _mapper.Map<List<AddressDto>>(addresses);
            return AppResult<List<AddressDto>>.Success(addressDtos);
        }
    }
}