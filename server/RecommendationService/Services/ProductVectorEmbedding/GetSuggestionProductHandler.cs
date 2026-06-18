using MassTransit.Initializers;
using MediatR;
using RecommendationService.Persistence;
using Shared.Core.EF.Application;
using RecommendationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecommendationService.Services.ProductVectorEmbedding
{
    public class GetSuggestionProductHandler : IRequestHandler<GetSuggestionProductQuery, AppResult<List<string>>>
    {
        private readonly IRecommandationUnitOfWork _unitOfWork;
        private readonly GrpcProductClient _rpcProductClient;
        public GetSuggestionProductHandler(IRecommandationUnitOfWork unitOfWork, GrpcProductClient rpcProductClient )
        {
            _unitOfWork = unitOfWork;
            _rpcProductClient = rpcProductClient;
        }

        public async Task<AppResult<List<string>>> Handle(GetSuggestionProductQuery request, CancellationToken cancellationToken)
        {
            if (request.UserId == null)
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

            var productTrackingWeight = new Dictionary<string, float>();//weight of each product in the user tracking list
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
                {
                    productTrackingWeight[item.ProductId] += weight;
                }
                else
                {
                    productTrackingWeight.Add(item.ProductId, weight);
                }
                hashSetProductId.Add(item.ProductId);
            }

            var productEmbedVectors = await _unitOfWork.ProductVectorEmbeddingRepository.GetProductVectorEmbeddingsByProductIds(hashSetProductId, cancellationToken);
            var inputVectors = new List<List<float>>();
            foreach (var item in productEmbedVectors)
            {
                var tmpVector = MultiplyWithWeight(item.Embedding, productTrackingWeight[item.ProductId]);
                inputVectors.Add(tmpVector);
            }
            var avgEmbedVector = ComputeAverageVector(inputVectors);
            var allVectorsWithProduct = await _unitOfWork.ProductVectorEmbeddingRepository.GetAll().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);    

            var resultsVectors = allVectorsWithProduct
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    Score = CosineSimilarity(avgEmbedVector, p.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(10);

            return AppResult<List<string>>.Success(resultsVectors.Select(x => x.ProductId).ToList());
        }

        #region Helpers
        public List<float> MultiplyWithWeight(List<float> values, float weight)
        {
            return values.Select(v => v * weight).ToList();
        }

        public List<float> ComputeAverageVector(List<List<float>> vectors)
        {
            if (vectors == null || vectors.Count == 0)
                return new List<float>();

            int dimension = vectors[0].Count;
            var result = new float[dimension];

            foreach (var vec in vectors)
            {
                for (int i = 0; i < dimension; i++)
                {
                    result[i] += vec[i];
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                result[i] /= vectors.Count;
            }

            return result.ToList();
        }

        public static float CosineSimilarity(List<float> a, List<float> b)
        {
            if (a.Count != b.Count) throw new ArgumentException("Vectors must have the same length");

            float dot = 0f;
            float normA = 0f;
            float normB = 0f;

            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB)));
        }
        #endregion
    }
}
