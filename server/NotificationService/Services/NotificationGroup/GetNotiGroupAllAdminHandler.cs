using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroup
{
    public class GetNotiGroupAllAdminHandler : IRequestHandler<GetNotiGroupAllAdminQuery, AppResult<NotificationGroupDto>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetNotiGroupAllAdminHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<NotificationGroupDto>> Handle(GetNotiGroupAllAdminQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.NotificationGroupRepository
                .GetByNameAsync(NotificationGroups.AllAdminsNotiGroupName, cancellationToken);

            if (result == null)
                return AppResult<NotificationGroupDto>.Failure("Admin notification group not found", 404);

            return AppResult<NotificationGroupDto>.Success(_mapper.Map<NotificationGroupDto>(result));
        }
    }
}
