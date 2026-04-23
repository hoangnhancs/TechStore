using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommentService.DTOs;
using CommentService.Persistence;
using MediatR;
using Shared.Core.EF.Application;

namespace CommentService.Services.Comment
{
    public class GetListCommentsByProductIdHandler : IRequestHandler<GetListCommentsByProductIdQuery, AppResult<List<CommentDto>>>
    {
        private readonly ICommentUnitOfWork _unitOfWork;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMapper _mapper;
        public GetListCommentsByProductIdHandler(ICommentUnitOfWork unitOfWork, GrpcIdentityClient grpcIdentityClient, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _grpcIdentityClient = grpcIdentityClient;
            _mapper = mapper;
        }
        public async Task<AppResult<List<CommentDto>>> Handle(GetListCommentsByProductIdQuery request, CancellationToken cancellationToken)
        {
            var comments = (await _unitOfWork.CommentRepository.GetListAsync(
                predicate: c => c.ProductId == request.ProductId,
                cancellationToken: cancellationToken
            )).OrderByDescending(c => c.CreatedAt).ToList();

            if (comments == null || !comments.Any())
            {
                return AppResult<List<CommentDto>>.Success(new List<CommentDto>());
            }

            var commentDtos = _mapper.Map<List<CommentDto>>(comments);

            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var usersInfo = await _grpcIdentityClient.GetUsersByIds(userIds);
            var userInfoDict = usersInfo.ToDictionary(u => u.UserId, u => u);

            foreach (var commentDto in commentDtos)
            {
                if (userInfoDict.TryGetValue(commentDto.UserId, out var userInfo))
                {
                    commentDto.UserName = userInfo.UserName;
                    commentDto.UserImageUrl = userInfo.ImageUrl;
                }
            }
            return AppResult<List<CommentDto>>.Success(commentDtos);
        }
    }
}