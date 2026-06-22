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
    public class UpdateAddressHandler : IRequestHandler<UpdateAddressCommand, AppResult<AddressDto>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UpdateAddressHandler(IIdentityUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var addressEntity = await _unitOfWork.AddressRepository.GetByIdAsync(
                request.AddressId,
                cancellationToken
            );

            if (addressEntity == null)
                return AppResult<AddressDto>.Failure("Address not found", 404);

            if (addressEntity.UserId != request.UserId)
                return AppResult<AddressDto>.Failure("You cannot update this address", 403);
            if (request.Address.IsDefault == true)
            {
                await _unitOfWork.AddressRepository.SetOtherAddressNotDefaultAsync(request.UserId, request.AddressId, cancellationToken);
            }
            _mapper.Map(request.Address, addressEntity);
            _unitOfWork.AddressRepository.Update(addressEntity);
            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<AddressDto>.Success(_mapper.Map<AddressDto>(addressEntity));
        }
    }
}