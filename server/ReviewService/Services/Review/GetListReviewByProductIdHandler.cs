using AutoMapper;
using MediatR;
using ReviewService.DTOs;
using ReviewService.Persistence;
using Shared.Core.EF.Application;

namespace ReviewService.Services.Review
{
    public class GetListReviewByProductIdHandler : IRequestHandler<GetListReviewsByProductIdQuery, AppResult<List<ReviewDto>>>
    {
        private readonly IReviewUnitOfWork _unitOfWork;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMapper _mapper;

        public GetListReviewByProductIdHandler(IReviewUnitOfWork unitOfWork, GrpcIdentityClient grpcIdentityClient, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _grpcIdentityClient = grpcIdentityClient;
            _mapper = mapper;
        }

        public async Task<AppResult<List<ReviewDto>>> Handle(GetListReviewsByProductIdQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

            if (!reviews.Any())
                return AppResult<List<ReviewDto>>.Success([]);

            var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var usersInfo = await _grpcIdentityClient.GetUsersByIds(userIds);
            var userInfoDict = usersInfo.ToDictionary(u => u.UserId, u => u);

            foreach (var reviewDto in reviewDtos)
            {
                if (userInfoDict.TryGetValue(reviewDto.UserId, out var userInfo))
                {
                    reviewDto.UserName = userInfo.UserName;
                    reviewDto.UserDisplayName = userInfo.DisplayName;
                    reviewDto.UserImageUrl = userInfo.ImageUrl;
                }
            }

            return AppResult<List<ReviewDto>>.Success(reviewDtos);
        }
    }
}
