using AutoMapper;
using Contract.Review;
using MassTransit;
using MediatR;
using ReviewService.DTOs;
using ReviewService.Persistence;
using Shared.Core.EF.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReviewService.Services.Review
{
    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, AppResult<ReviewDto>>
    {
        
        private readonly IReviewUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        public CreateReviewHandler(IReviewUnitOfWork unitOfWork, 
            IMapper mapper, 
            GrpcIdentityClient grpcIdentityClient, 
            IPublishEndpoint publishEndpoint,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _grpcIdentityClient = grpcIdentityClient;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AppResult<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = new Entities.Review
            {
                ProductId = request.ProductId,
                UserId = request.UserId,
                Content = request.Content,
                Rating = request.Rating
            };
            await _unitOfWork.ReviewRepository.AddAsync(review);
            //var userIds = new List<string> { review.UserId };
            //var user = (await _grpcIdentityClient.GetUsersByIds(userIds)).FirstOrDefault();
            var user = _httpContextAccessor.HttpContext?.User;
            await _publishEndpoint.Publish(new ReviewCreated
            {
                ReviewId = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                Content = review.Content,
                Rating = review.Rating
            });
            var result = await _unitOfWork.CommitAsync();
            if (!result)
            {
                return AppResult<ReviewDto>.Failure("Failed to create review", 400);
            }
            var dto = _mapper.Map<ReviewDto>(review);

            dto.UserName = user?.FindFirst(ClaimTypes.Name)?.Value ?? request.UserName;
            dto.UserDisplayName = user?.FindFirst("display_name")?.Value ?? request.UserImageUrl; ;
            dto.UserImageUrl = user?.FindFirst("image_url")?.Value ?? request.UserImageUrl ?? request.UserImageUrl;
            return AppResult<ReviewDto>.Success(dto);
        }

    }
}