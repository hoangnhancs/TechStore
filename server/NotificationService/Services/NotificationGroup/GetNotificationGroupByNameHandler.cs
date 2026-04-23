using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var adminGroup = await _unitOfWork.NotificationGroupRepository.GetListAsync(
                q => q.Name == request.Name,
                cancellationToken: cancellationToken
            );

            if (adminGroup == null || !adminGroup.Any())
            {
                return AppResult<NotificationGroupDto>.Failure("Notification group not found", 404);
            }

            var adminGroupDto = _mapper.Map<NotificationGroupDto>(adminGroup.FirstOrDefault());
            return AppResult<NotificationGroupDto>.Success(adminGroupDto);
        }
    }
}