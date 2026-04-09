using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Persistence;
using MediatR;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address
{
    public class DeleteAddressHandler : IRequestHandler<DeleteAddressCommand, AppResult<Unit>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        public DeleteAddressHandler(IIdentityUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _unitOfWork.AddressRepository.GetByIdAsync(request.AddressId);
            if (address == null || address.UserId != request.UserId)
            {
                return AppResult<Unit>.Failure("Address not found or does not belong to the user.", 404);
            }
            _unitOfWork.AddressRepository.Delete(address);
            var result = await _unitOfWork.CommitAsync(cancellationToken);
            if (!result) return AppResult<Unit>.Failure("Problem when delete address", 400);
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}