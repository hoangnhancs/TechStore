using System;
using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using CartService.Repositories.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            p => p.UserId == request.UserId,
            q => q.Include(b => b.Items),
            cancellationToken: cancellationToken
        )).FirstOrDefault();

        if (basket == null)
        {
            basket = new Entities.Basket
            {
                UserId = request.UserId,
                Items = new List<Entities.BasketItem>()
            };

            await _unitOfWork.BasketRepository.AddAsync(basket, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return AppResult<BasketDto>.Success(_mapper.Map<BasketDto>(basket));
    }
}