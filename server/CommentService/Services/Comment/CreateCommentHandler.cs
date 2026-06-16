using AutoMapper;
using CommentService.DTOs;
using CommentService.Persistence;
using Contract.Comment;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Core.EF.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommentService.Services.Comment
{
    /// <summary>
    /// new comment: is admin -> no notification, is not admin -> send notification
    /// reply comment: is self reply -> no notification, is not self reply -> send notification
    /// </summary>
    public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, AppResult<CommentDto>>
    {
        private readonly ICommentUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CreateCommentHandler(ICommentUnitOfWork unitOfWork, IMapper mapper, IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AppResult<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = new Entities.Comment
            {
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                UserId = request.UserId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CommentRepository.AddAsync(comment);

            var isReply = !string.IsNullOrWhiteSpace(comment.ParentCommentId);

            var isAdminNewComment = comment.ParentCommentId == null && (_httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false);

            string? parentCommentUserId = null;

            if (isReply)
            {
                parentCommentUserId = await _unitOfWork.CommentRepository
                    .GetAll()
                    .Where(x => x.Id == comment.ParentCommentId)
                    .Select(x => x.UserId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var isSelfReply =
                isReply &&
                !string.IsNullOrEmpty(parentCommentUserId) &&
                parentCommentUserId == comment.UserId;
            if (!isSelfReply && !isAdminNewComment)
            {
                await _publishEndpoint.Publish(
                    new CommentCreated
                    {
                        Title = comment.Content,
                        ReferenceId = comment.ReferenceId,
                        ReferenceType = comment.ReferenceType,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        CreatedAt = comment.CreatedAt,
                        ParentCommentId = comment.ParentCommentId
                    },
                    cancellationToken);
            }

            var success = await _unitOfWork.CommitAsync();

            if (!success)
            {
                return AppResult<CommentDto>.Failure(
                    "Failed to create comment",
                    400);
            }

            var dto = _mapper.Map<CommentDto>(comment);

            dto.UserName = request.UserName;
            dto.UserImageUrl = request.UserImageUrl;

            return AppResult<CommentDto>.Success(dto);
        }
    }
}