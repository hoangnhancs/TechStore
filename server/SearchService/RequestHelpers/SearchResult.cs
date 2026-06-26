using SearchService.Entities;

namespace SearchService.RequestHelpers;

public class SearchResult
{
    public List<ProductItem> Results { get; set; } = [];
    public List<int> RelatedCats { get; set; } = [];
    public int PageCount { get; set; }
    public long TotalCount { get; set; }
}
