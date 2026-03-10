using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SearchService.RequestHelpers
{
    public class SearchParams
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string OrderBy { get; set; } = string.Empty;
        [ModelBinder(BinderType = typeof(ListIntModelBinder))]
        public List<int> FilterTagValues { get; set; } = [];
    }

    public enum OrderProps
    {
        Discount,
        PriceAsc,
        PriceDesc,
        Newest,
    }
}