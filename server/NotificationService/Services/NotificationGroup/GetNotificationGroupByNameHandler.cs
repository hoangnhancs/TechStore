using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroup
{
    public class GetNotificationGroupByNameHandler : IRequestHandler<GetNotificationGroupByNameQuery, AppResult<NotificationGroupDto>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetNotificationGroupByNameHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<NotificationGroupDto>> Handle(GetNotificationGroupByNameQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.NotificationGroupRepository
                .GetByNameAsync(request.Name, cancellationToken);

            if (group == null)
                return AppResult<NotificationGroupDto>.Failure("Notification group not found", 404);

            return AppResult<NotificationGroupDto>.Success(_mapper.Map<NotificationGroupDto>(group));
        }
    }
}
