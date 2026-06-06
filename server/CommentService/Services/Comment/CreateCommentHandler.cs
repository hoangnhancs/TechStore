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
    public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, AppResult<CommentDto>>
    {
        private readonly ICommentUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        public CreateCommentHandler(ICommentUnitOfWork unitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
        }
        public async Task<AppResult<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = new Entities.Comment
            {
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = request.ParentCommentId
            };
            await _unitOfWork.CommentRepository.AddAsync(comment);
            var userIds = new List<string> { comment.UserId };
            // var users = await _grpcIdentityClient.GetUsersByIds(userIds);
            var result = await _unitOfWork.CommitAsync();
            if (!result)
            {
                return AppResult<CommentDto>.Failure("Failed to create comment", 400);
            }
            var dto = _mapper.Map<CommentDto>(comment);
            dto.UserName = request.UserName;
            dto.UserImageUrl = request.UserImageUrl;
            return AppResult<CommentDto>.Success(dto);
        }
    }
}