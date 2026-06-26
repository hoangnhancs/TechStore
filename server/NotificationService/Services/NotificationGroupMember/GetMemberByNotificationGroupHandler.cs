using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroupMember
{
    public class GetMemberByNotificationGroupHandler : IRequestHandler<GetMemberByNotificationGroupQuery, AppResult<List<NotificationGroupMemberDto>>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMemberByNotificationGroupHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<List<NotificationGroupMemberDto>>> Handle(GetMemberByNotificationGroupQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.NotificationGroupRepository
                .GetByIdWithMembersAsync(request.GroupId, cancellationToken);

            if (group == null)
                return AppResult<List<NotificationGroupMemberDto>>.Failure("Notification group not found.", 404);

            return AppResult<List<NotificationGroupMemberDto>>.Success(
                _mapper.Map<List<NotificationGroupMemberDto>>(group.Members));
        }
    }
}
