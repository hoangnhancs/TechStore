using MediatR;
using RecommendationService.Persistence;
using Shared.Core.EF.Application;
using RecommendationService.Entities;

namespace RecommendationService.Services.ProductVectorEmbedding
{
    public class GetSuggestionProductHandler : IRequestHandler<GetSuggestionProductQuery, AppResult<List<string>>>
    {
        private readonly IRecommendationUnitOfWork _unitOfWork;
        private readonly GrpcProductClient _rpcProductClient;

        public GetSuggestionProductHandler(IRecommendationUnitOfWork unitOfWork, GrpcProductClient rpcProductClient)
        {
            _unitOfWork = unitOfWork;
            _rpcProductClient = rpcProductClient;
        }

        public async Task<AppResult<List<string>>> Handle(GetSuggestionProductQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                var products = await _rpcProductClient.GetTop10SoldProduct();
                return AppResult<List<string>>.Success(products.Select(x => x.Id).ToList());
            }

            var userTracking = await _unitOfWork.UserActionTrackingRepository.GetUserActionTrackingByUserId(request.UserId, cancellationToken);

            if (!userTracking.Any())
            {
                var products = await _rpcProductClient.GetTop10SoldProduct();
                return AppResult<List<string>>.Success(products.Select(x => x.Id).ToList());
            }

            var productTrackingWeight = new Dictionary<string, float>();
            var hashSetProductId = new HashSet<string>();

            foreach (var item in userTracking)
            {
                float weight = item.ActionType switch
                {
                    UserActionType.View => 1f,
                    UserActionType.AddToCart => 1.5f,
                    UserActionType.Purchase => 2f,
                    _ => 0f
                };
                if (weight == 0f) continue;

                if (productTrackingWeight.ContainsKey(item.ProductId))
                    productTrackingWeight[item.ProductId] += weight;
                else
                    productTrackingWeight.Add(item.ProductId, weight);

                hashSetProductId.Add(item.ProductId);
            }

            var productEmbedVectors = await _unitOfWork.ProductVectorEmbeddingRepository
                .GetProductVectorEmbeddingsByProductIds(hashSetProductId, cancellationToken);

            var inputVectors = new List<float[]>();
            foreach (var item in productEmbedVectors)
            {
                var raw = item.Embedding.Memory.ToArray();
                var weighted = MultiplyWithWeight(raw, productTrackingWeight[item.ProductId]);
                inputVectors.Add(weighted);
            }

            var avgEmbedVector = ComputeAverageVector(inputVectors);
            var topSimilarProducts = await _unitOfWork.ProductVectorEmbeddingRepository
                .GetTopSimilarProductsAsync(avgEmbedVector, request.NumberTopProduct, cancellationToken);

            return AppResult<List<string>>.Success(topSimilarProducts);
        }

        private static float[] MultiplyWithWeight(float[] values, float weight)
            => values.Select(v => v * weight).ToArray();

        private static List<float> ComputeAverageVector(List<float[]> vectors)
        {
            if (vectors.Count == 0) return [];

            int dimension = vectors[0].Length;
            var result = new float[dimension];

            foreach (var vec in vectors)
                for (int i = 0; i < dimension; i++)
                    result[i] += vec[i];

            for (int i = 0; i < dimension; i++)
                result[i] /= vectors.Count;

            return [.. result];
        }
    }
}
