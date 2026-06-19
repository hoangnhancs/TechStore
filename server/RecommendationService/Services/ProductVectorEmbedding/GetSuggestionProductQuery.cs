using MediatR;
using Shared.Core.EF.Application;

namespace RecommendationService.Services.ProductVectorEmbedding
{
    public class GetSuggestionProductQuery : IRequest<AppResult<List<string>>>
    {
        public string? UserId { get; set; }
        public int NumberTopProduct { get; set; } = 10;
    }
}
