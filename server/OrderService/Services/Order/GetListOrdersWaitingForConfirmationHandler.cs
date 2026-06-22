using AutoMapper;
using MediatR;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class GetListOrdersWaitingForConfirmationHandler : IRequestHandler<GetListOrdersWaitingForConfirmationQuery, AppResult<List<OrderWithUserInforDto>>>
    {
        private readonly IOrderUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetListOrdersWaitingForConfirmationHandler(IOrderUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<List<OrderWithUserInforDto>>> Handle(GetListOrdersWaitingForConfirmationQuery request, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.OrderRepository.GetListOrdersWaitingForConfirmation();
            var orderDtos = _mapper.Map<List<OrderWithUserInforDto>>(orders);
            return AppResult<List<OrderWithUserInforDto>>.Success(orderDtos);
        }
    }
}
