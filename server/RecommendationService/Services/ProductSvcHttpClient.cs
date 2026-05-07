using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecommendationService.Models;

namespace RecommendationService.Services
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
            var items = await _httpClient.GetFromJsonAsync<List<ProductItem>>(
                $"{_configuration["ProductServiceUrl"]}/api/products/all"
            ) ?? new List<ProductItem>();

            items.Select(i => { i.UpdateAttributeText(); return i; }).ToList();
            return items;
        }
    }
}