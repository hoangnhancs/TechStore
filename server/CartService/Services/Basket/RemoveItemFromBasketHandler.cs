using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket
{
    public class RemoveItemFromBasketHandler : IRequestHandler<RemoveItemFromBasketCommand, AppResult<Unit>>
    {
        private readonly ICartUnitOfWork _unitOfWork;
        public RemoveItemFromBasketHandler(ICartUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(RemoveItemFromBasketCommand request, CancellationToken cancellationToken)
        {
            var basket = (await _unitOfWork.BasketRepository.GetListAsync(
                p => p.UserId == request.UserId,
                q => q.Include(b => b.Items),
                cancellationToken: cancellationToken
            )).FirstOrDefault();

            if (basket == null)
                return AppResult<Unit>.Failure("Basket not found", 404);

            basket.RemoveItem(request.ProductId, request.Quantity);
            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}