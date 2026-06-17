using MassTransit.Initializers;
using MediatR;
using RecommendationService.Persistence;
using Shared.Core.EF.Application;

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
            throw new NotImplementedException();
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
