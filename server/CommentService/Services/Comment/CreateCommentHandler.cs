using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommentService.DTOs;
using CommentService.Persistence;
using Contract.Comment;
using MassTransit;
using MediatR;
using Shared.Core.EF.Application;

namespace CommentService.Services.Comment
{
    public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, AppResult<CommentDto>>
    {
        private readonly ICommentUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        public CreateCommentHandler(ICommentUnitOfWork unitOfWork, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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
            await _publishEndpoint.Publish(new CommentCreated
            {
                Title = comment.Content,
                ReferenceId = comment.ReferenceId,
                ReferenceType = comment.ReferenceType,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                ParentCommentId = comment.ParentCommentId
            });
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