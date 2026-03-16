using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<ProductItem>()
                .Key(x => x.CategoryId, KeyType.Ascending)
                .Key(x => x.BrandId, KeyType.Ascending)
                .CreateAsync();

            await DB.Index<ProductItem>()
                .Key(x => x.Price, KeyType.Ascending)
                .CreateAsync();

            await DB.Index<ProductItem>()
                .Key(x => x.CreatedAt, KeyType.Ascending)
                .CreateAsync();

            await DB.Index<ProductItem>()
                .Key(x => x.Name, KeyType.Text)
                .Key(x => x.Description, KeyType.Text)
                .Key(x => x.AttributeText, KeyType.Text)
                // .Key(x => x.MetaDescription, KeyType.Text)
                // .Key(x => x.MetaKeywords, KeyType.Text)
                // .Key(x => x.MetaTitle, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<ProductItem>();

            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<ProductSvcHttpClient>();
            //không dùng Grpc vì sẽ phụ thuộc vào productsvc
            // var grpcClient = scope.ServiceProvider.GetRequiredService<GrpcProductClient>();
            Console.WriteLine($"Starting get data from Product Service... Current items in DB: {count}");
            var latestItem = await DB.Find<ProductItem>()
                .Sort(x => x.Descending(p => p.UpdatedAt))
                .ExecuteFirstAsync();
            var lastUpdated = latestItem?.UpdatedAt;
            var items = await httpClient.GetItemsForSearchDb();
            // var items = grpcClient.GetUpdatedProduct(lastUpdated);



            Console.WriteLine($"Products in DB: {items.Count}");

            if (items.Count > 0)
            {
                Console.WriteLine("Updating database...");
                await DB.SaveAsync(items);
                Console.WriteLine("Database updated");
            }
        }
    }
}