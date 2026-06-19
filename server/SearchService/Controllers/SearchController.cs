using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;
using SearchService.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly GrpcRecommendationClient _grpcRecommendationClient;

        public SearchController(GrpcRecommendationClient grpcRecommendationClient)
        {
            _grpcRecommendationClient = grpcRecommendationClient;
        }
        [HttpGet("{categoryId?}/{brandId?}")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchItems([FromRoute] int? categoryId, [FromRoute] int? brandId, [FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<ProductItem, ProductItem>();

            if (categoryId.HasValue)
            {
                query = query.Match(x => x.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                query = query.Match(x => x.BrandId == brandId.Value);
            }

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            query = searchParams.OrderBy switch
            {
                "priceasc" => query.Sort(x => x.Ascending(a => a.Price)),
                "pricedesc" => query.Sort(x => x.Descending(a => a.Price)),
                "discount" => query.Sort(x => x.Descending(a => a.DiscountPercentage)),
                "newest" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.CreatedAt))
            };

            if (searchParams.FilterTagValues.Any())
            {
                query = query.Match(x => x.ProductFilterTagValues.Any(ftv => searchParams.FilterTagValues.Contains(ftv.FilterTagValueId)));
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);
            var result = await query.ExecuteAsync();
            List<int> relatedCats = result.Results
                .Select(i => i.CategoryId)
                .Distinct()
                .ToList();
            return Ok(new
            {
                results = result.Results,
                relatedCats = relatedCats,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }

        [HttpGet("top10")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductItem>>> GetTop10Items()
        {
            var categories = await DB.Find<ProductItem, int>()
                .Project(i => i.CategoryId)
                .ExecuteAsync();

            var distinctCategories = categories.Distinct().ToList();
            
            var result = new List<ProductItem>();
            
            foreach (var categoryId in distinctCategories)
            {
                var topItems = await DB.Find<ProductItem>()
                    .Match(i => i.CategoryId == categoryId)
                    .Sort(x => x.Descending(i => i.UnitSold))
                    .Limit(10)
                    .ExecuteAsync();

                result.AddRange(topItems);
            }

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
    }
}