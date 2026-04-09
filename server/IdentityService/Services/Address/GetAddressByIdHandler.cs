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
    public class GetAddressByIdHandler : IRequestHandler<GetAddressByIdQuery, AppResult<AddressDto>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAddressByIdHandler(IIdentityUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<AddressDto>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var address = await _unitOfWork.AddressRepository.GetByIdAsync(request.AddressId);
            if (address == null || address.UserId != request.UserId)
                return AppResult<AddressDto>.Failure("Address not found", 404);
            var addressDto = _mapper.Map<AddressDto>(address);
            return AppResult<AddressDto>.Success(addressDto);
        }
    }
}