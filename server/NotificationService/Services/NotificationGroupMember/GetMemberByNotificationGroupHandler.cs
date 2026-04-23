using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            var group = await _unitOfWork.NotificationGroupRepository.GetByIdAsync(
                id: request.GroupId,
                query: ng => ng.Include(g => g.Members),
                cancellationToken: cancellationToken
            );

            if (group == null)
            {
                return AppResult<List<NotificationGroupMemberDto>>.Failure("Notification group not found.", 404);
            }

            var memberDtos = _mapper.Map<List<NotificationGroupMemberDto>>(group.Members);

            return AppResult<List<NotificationGroupMemberDto>>.Success(memberDtos);
        }
    }
}