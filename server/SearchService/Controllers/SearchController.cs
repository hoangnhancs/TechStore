using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;
using SearchService.Services;
using System.Security.Claims;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly GrpcRecommendationClient _grpcRecommendationClient;
        private readonly ICacheService _cache;

        public SearchController(GrpcRecommendationClient grpcRecommendationClient, ICacheService cache)
        {
            _grpcRecommendationClient = grpcRecommendationClient;
            _cache = cache;
        }

        [HttpGet("{categoryId?}/{brandId?}")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchItems([FromRoute] int? categoryId, [FromRoute] int? brandId, [FromQuery] SearchParams searchParams)
        {
            // Cache only pure category browse: no brand, no text search, no tag filters
            bool isCategoryOnly = categoryId.HasValue
                && !brandId.HasValue
                && string.IsNullOrEmpty(searchParams.SearchTerm)
                && searchParams.FilterTagValues.Count == 0;

            if (isCategoryOnly)
            {
                var cacheKey = $"search:category:{categoryId}:{searchParams.OrderBy}:{searchParams.PageNumber}:{searchParams.PageSize}";
                var cached = await _cache.GetAsync<SearchResult>(cacheKey);
                if (cached != null) return Ok(cached);

                var result = await ExecuteSearchAsync(categoryId, null, searchParams);
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                return Ok(result);
            }

            return Ok(await ExecuteSearchAsync(categoryId, brandId, searchParams));
        }

        [HttpGet("top10")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductItem>>> GetTop10Items()
        {
            const string cacheKey = "search:top10";
            var cached = await _cache.GetAsync<List<ProductItem>>(cacheKey);
            if (cached != null) return Ok(cached);

            var categories = await DB.Find<ProductItem, int>()
                .Project(i => i.CategoryId)
                .ExecuteAsync();

            var distinctCategories = categories.Distinct().ToList();
            var result = new List<ProductItem>();

            foreach (var catId in distinctCategories)
            {
                var topItems = await DB.Find<ProductItem>()
                    .Match(i => i.CategoryId == catId)
                    .Sort(x => x.Descending(i => i.UnitSold))
                    .Limit(10)
                    .ExecuteAsync();

                result.AddRange(topItems);
            }

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return Ok(result);
        }

        [HttpGet("suggestion")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductItem>>> GetSuggestProduct(int numberOfProduct = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _grpcRecommendationClient.GetSuggestProduct(userId, numberOfProduct);
            return Ok(result);
        }

        private static async Task<SearchResult> ExecuteSearchAsync(int? categoryId, int? brandId, SearchParams searchParams)
        {
            var query = DB.PagedSearch<ProductItem, ProductItem>();

            if (categoryId.HasValue)
                query = query.Match(x => x.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Match(x => x.BrandId == brandId.Value);

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();

            query = searchParams.OrderBy switch
            {
                "priceasc" => query.Sort(x => x.Ascending(a => a.Price)),
                "pricedesc" => query.Sort(x => x.Descending(a => a.Price)),
                "discount" => query.Sort(x => x.Descending(a => a.DiscountPercentage)),
                "newest" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.CreatedAt))
            };

            if (searchParams.FilterTagValues.Count > 0)
                query = query.Match(x => x.ProductFilterTagValues.Any(ftv => searchParams.FilterTagValues.Contains(ftv.FilterTagValueId)));

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();
            return new SearchResult
            {
                Results = [.. result.Results],
                RelatedCats = result.Results.Select(i => i.CategoryId).Distinct().ToList(),
                PageCount = result.PageCount,
                TotalCount = result.TotalCount
            };
        }
    }
}
