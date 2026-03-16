using System;
using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using CartService.Repositories.Interface;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class GetBasketHandler : IRequestHandler<GetBasketQuery, AppResult<BasketDto>>
{
    private readonly ICartUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBasketHandler(IMapper mapper, ICartUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = (await _unitOfWork.BasketRepository.GetListAsync(
            p => p.UserId == request.UserId
        )).FirstOrDefault();

        if (basket == null)
        {
            basket = new Entities.Basket
            {
                UserId = request.UserId,
                Items = new List<Entities.BasketItem>()
            };
        }

        return AppResult<BasketDto>.Success(_mapper.Map<BasketDto>(basket));
    }
}