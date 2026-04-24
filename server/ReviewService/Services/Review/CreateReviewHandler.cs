using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ReviewService.DTOs;
using ReviewService.Persistence;
using Shared.Core.EF.Application;

namespace ReviewService.Services.Review
{
    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, AppResult<ReviewDto>>
    {
        
        private readonly IReviewUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        public CreateReviewHandler(IReviewUnitOfWork unitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
        }
        public async Task<AppResult<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = new Entities.Review
            {
                ProductId = request.ProductId,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                Rating = request.Rating
            };
            await _unitOfWork.ReviewRepository.AddAsync(review);
            var userIds = new List<string> { review.UserId };
            // var users = await _grpcIdentityClient.GetUsersByIds(userIds);
            var result = await _unitOfWork.CommitAsync();
            if (!result)
            {
                return AppResult<ReviewDto>.Failure("Failed to create review", 400);
            }
            var dto = _mapper.Map<ReviewDto>(review);

            dto.UserName = request.UserName;
            dto.UserImageUrl = request.UserImageUrl;
            return AppResult<ReviewDto>.Success(dto);
        }

    }
}