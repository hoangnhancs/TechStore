using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class GetNotificationGroupsByUserIdHandler : IRequestHandler<GetNotificationGroupsByUserIdQuery, AppResult<List<NotificationGroupDto>>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetNotificationGroupsByUserIdHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<List<NotificationGroupDto>>> Handle(GetNotificationGroupsByUserIdQuery request, CancellationToken cancellationToken)
        {
            await _unitOfWork.NotificationRepository.Ge
        }
    }
}