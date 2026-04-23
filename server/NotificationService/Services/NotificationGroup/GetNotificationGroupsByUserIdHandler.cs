using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Org.BouncyCastle.Tls;
using Shared.Core.EF.Application;

namespace NotificationService.Services.NotificationGroup
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
            var groups = await _unitOfWork.NotificationGroupRepository.GetListAsync(
                predicate: ng => ng.Members.Any(m => m.UserId == request.UserId),
                cancellationToken: cancellationToken
            );

            var groupDtos = _mapper.Map<List<NotificationGroupDto>>(groups);
            return AppResult<List<NotificationGroupDto>>.Success(groupDtos);
        }
    }
}