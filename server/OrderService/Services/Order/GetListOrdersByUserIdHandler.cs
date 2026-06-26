using AutoMapper;
using MediatR;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class GetListOrdersByUserIdHandler : IRequestHandler<GetListOrdersByUserIdQuery, AppResult<List<OrderDto>>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetListOrdersByUserIdHandler(IMapper mapper, IOrderUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<List<OrderDto>>> Handle(GetListOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.OrderRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);

        if (orders.Count == 0)
            return AppResult<List<OrderDto>>.Success([]);

        return AppResult<List<OrderDto>>.Success(orders.Select(_mapper.Map<OrderDto>).ToList());
    }
}
