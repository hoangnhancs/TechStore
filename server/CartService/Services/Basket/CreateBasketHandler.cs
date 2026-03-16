using System;
using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using CartService.Repositories.Interface;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class CreateBasketHandler : IRequestHandler<CreateBasketCommand, AppResult<BasketDto>>
{
    private readonly ICartUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public CreateBasketHandler(ICartUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<AppResult<BasketDto>> Handle(CreateBasketCommand request, CancellationToken cancellationToken)
    {
        var newBasket = new CartService.Entities.Basket()
        {
            UserId = request.UserId
        };
        await _unitOfWork.BasketRepository.AddAsync(newBasket, cancellationToken);
        var result = await _unitOfWork.CommitAsync(cancellationToken);
        if (!result) return AppResult<BasketDto>.Failure("Problem when create basket", 400);
        return AppResult<BasketDto>.Success(_mapper.Map<BasketDto>(newBasket));
    }
}
