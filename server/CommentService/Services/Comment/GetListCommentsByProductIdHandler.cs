using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommentService.DTOs;
using CommentEntity = CommentService.Entities.Comment;
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
            // Load all comments flat (parents + replies at all levels) in one query
            var allComments = (await _unitOfWork.CommentRepository.GetListAsync(
                predicate: c => c.ReferenceId == request.ProductId
                    && c.ReferenceType == CommentEntity.ReferenceTypes.Product.ToString(),
                cancellationToken: cancellationToken
            )).ToList();

            if (allComments.Count == 0)
                return AppResult<List<CommentDto>>.Success([]);

            var allDtos = _mapper.Map<List<CommentDto>>(allComments);

            var userIds = allDtos.Select(c => c.UserId).Distinct().ToList();
            var usersInfo = await _grpcIdentityClient.GetUsersByIds(userIds);
            var userInfoDict = usersInfo.ToDictionary(u => u.UserId, u => u);

            foreach (var dto in allDtos)
            {
                if (userInfoDict.TryGetValue(dto.UserId, out var info))
                {
                    dto.UserDisplayName = info.DisplayName;
                    dto.UserImageUrl = info.ImageUrl;
                }
            }

            // Build tree in memory — handles unlimited nesting depth.
            // Must clear Replies first: EF relationship fixup auto-links navigation
            // properties for tracked entities fetched in the same query, and AutoMapper
            // recursively maps them → duplicates if we don't reset before re-building.
            var dtoDict = allDtos.ToDictionary(d => d.Id);
            foreach (var dto in allDtos)
                dto.Replies.Clear();

            var roots = new List<CommentDto>();
            foreach (var dto in allDtos)
            {
                if (dto.ParentCommentId == null)
                    roots.Add(dto);
                else if (dtoDict.TryGetValue(dto.ParentCommentId, out var parent))
                    parent.Replies.Add(dto);
            }

            roots = [.. roots.OrderByDescending(c => c.CreatedAt)];
            return AppResult<List<CommentDto>>.Success(roots);
        }
    }
}