using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using NotificationService.Repositories.Interfaces;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroup
{
    public class GetNotificationGroupByIdHandler : IRequestHandler<GetNotificationGroupByIdQuery, AppResult<NotificationGroupDto>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetNotificationGroupByIdHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<NotificationGroupDto>> Handle(GetNotificationGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.NotificationGroupRepository.GetByIdAsync(request.GroupId);
            if (group == null)
            {
                return AppResult<NotificationGroupDto>.Failure("Notification group not found", 404);
            }

            var groupDto = _mapper.Map<NotificationGroupDto>(group);
            return AppResult<NotificationGroupDto>.Success(groupDto);
        }
    }
}