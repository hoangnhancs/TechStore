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
    public class AddAddressHandler : IRequestHandler<AddAddressCommand, AppResult<AddressDto>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AddAddressHandler(IIdentityUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<AddressDto>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
        {
            if (request.Address.IsDefault == true)
            {
                await _unitOfWork.AddressRepository.SetOtherAddressNotDefaultAsync(request.UserId, cancellationToken);
            }
            var addressEntity = _mapper.Map<Entities.Address>(request.Address);
            addressEntity.UserId = request.UserId;
            await _unitOfWork.AddressRepository.AddAsync(addressEntity, cancellationToken);
            var result = await _unitOfWork.CommitAsync(cancellationToken);
            if (!result) return AppResult<AddressDto>.Failure("Problem when create address", 400);
            return AppResult<AddressDto>.Success(_mapper.Map<AddressDto>(addressEntity));
        }
    }
}