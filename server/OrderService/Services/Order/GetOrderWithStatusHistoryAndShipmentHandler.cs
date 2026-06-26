using AutoMapper;
using MediatR;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;
using System.Security.Claims;

namespace OrderService.Services.Order
{
    public class GetOrderWithStatusHistoryAndShipmentHandler : IRequestHandler<GetOrderWithStatusHistoryAndShipmentQuery, AppResult<OrderDto>>
    {
        private readonly IOrderUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetOrderWithStatusHistoryAndShipmentHandler(IOrderUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AppResult<OrderDto>> Handle(GetOrderWithStatusHistoryAndShipmentQuery request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdWithHistoriesAndShipmentAsync(request.OrderId, cancellationToken);

            if (order == null)
                return AppResult<OrderDto>.Failure("Order not found", 404);

            var roles = _httpContextAccessor.HttpContext?.User
                .FindAll(ClaimTypes.Role)
                .Select(x => x.Value.ToLower())
                .ToList();

            if (order.UserId != request.UserId && (roles == null || !roles.Contains("admin")))
                return AppResult<OrderDto>.Failure("Unauthorized access to order history", 403);

            return AppResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }
    }
}
