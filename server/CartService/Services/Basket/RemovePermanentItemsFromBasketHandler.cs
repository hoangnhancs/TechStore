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
    public class RemovePermanentItemsFromBasketHandler : IRequestHandler<RemovePermanentItemsFromBasketCommand, AppResult<Unit>>
    {
        private readonly ICartUnitOfWork _unitOfWork;
        public RemovePermanentItemsFromBasketHandler(ICartUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(RemovePermanentItemsFromBasketCommand request, CancellationToken cancellationToken)
        {
            var basket = (await _unitOfWork.BasketRepository.GetListAsync(
                p => p.UserId == request.UserId,
                q => q.Include(b => b.Items),
                cancellationToken: cancellationToken
            )).FirstOrDefault();

            if (basket == null)
                return AppResult<Unit>.Failure("Basket not found", 404);

            foreach(var id in request.ProductIds)
            {
                basket.RemovePermanentItem(id);
            }
            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<Unit>.Success(Unit.Value);

        }
    }
}