using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Services
{
    public class ProductSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ProductSvcHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<List<ProductItem>> GetItemsForSearchDb()
        {
            var lastUpdated = await DB.Find<ProductItem, string>()
                .Sort(x => x.Descending(p => p.UpdatedAt))
                .Project(x => x.UpdatedAt.ToString())
                .ExecuteFirstAsync();

            var items = await _httpClient.GetFromJsonAsync<List<ProductItem>>(
                $"{_configuration["ProductServiceUrl"]}/api/products/all?lastUpdated={lastUpdated}"
            ) ?? new List<ProductItem>();

            items.Select(i => { i.UpdateAttributeText(); return i; }).ToList();
            return items;
        }
    }
}