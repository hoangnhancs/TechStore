using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Entities;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Data
{
    public class DbInitializer
    {
        public class BrandGroup
        {
            public int categoryId { get; set; }
            public required string category { get; set; }
            public List<BrandDto> brands { get; set; } = [];
        }

        public class BrandDto
        {
            public required string name { get; set; }
            public required string imageUrl { get; set; }
        }

        public static async Task SeedData(ProductSvcDbContext context, ILogger logger)
        {

            List<string> categories = new List<string> { "camera", "laptop", "microphone", "monitor", "pc", "phone", "printer", "tablet", "tv", "watch" };
            List<string> categoryNames = new List<string> { "Camera", "Laptop", "Mic thu âm", "Màn hình", "PC", "Điện thoại", "Máy in", "Máy tính bảng", "Tivi", "Đồng hồ" };
            logger.LogInformation("Seeding categories...");
            if (!context.Categories.Any())
            {
                var categoryEntities = categories
                    .Select((name, idx) => new Category
                    {
                        Name = name,
                        DisplayName = categoryNames[idx]
                    })
                    .ToList();
                await context.Categories.AddRangeAsync(categoryEntities);
                await context.SaveChangesAsync();
                logger.LogInformation($"Added {categoryEntities.Count} categories.");
            }
            var categoriesInDb = context.Categories.ToList();

            // Use project root/Data/Jsons instead of bin/Debug
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Jsons");
            logger.LogInformation($"Base path for JSONs: {basePath}");
            string jsonContent = File.ReadAllText(Path.Combine(basePath, "tags_dict.json"));
            var tagsDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(jsonContent);

            //add filtertag
            logger.LogInformation("Seeding filter tags...");
            if (!context.FilterTags.Any())
            {
                if (tagsDict == null)
                {
                    logger.LogError("Failed to deserialize tags dictionary from JSON file.");
                    throw new Exception("Failed to deserialize tags dictionary from JSON file.");
                }

                foreach (var tag in tagsDict)
                {
                    var matchedCat = categoriesInDb.FirstOrDefault(c => c.Name == tag.Key);
                    if (matchedCat == null)
                    {
                        logger.LogError($"Category '{tag.Key}' not found in the database for filter tag.");
                        throw new Exception($"Category '{tag.Key}' not found in the database.");
                    }
                    var filterTags = tag.Value;
                    foreach (var filterTag in filterTags)
                    {
                        var tagEntity = new FilterTag
                        {
                            Name = filterTag.Key,
                            CategoryId = matchedCat.Id
                        };
                        await context.FilterTags.AddAsync(tagEntity);
                        await context.SaveChangesAsync();
                        var matchedTagFilter = await context.FilterTags.FirstOrDefaultAsync(ft => ft.Name == filterTag.Key && ft.CategoryId == matchedCat.Id);
                        var tagValues = filterTag.Value;
                        var tagValuesEntity = new List<FilterTagValue>();
                        foreach (var tagValue in tagValues)
                        {
                            if (matchedTagFilter == null)
                            {
                                logger.LogError($"FilterTag '{filterTag.Key}' not found for category '{matchedCat.Name}'.");
                                throw new Exception($"FilterTag '{filterTag.Key}' not found for category '{matchedCat.Name}'.");
                            }
                            tagValuesEntity.Add(new FilterTagValue
                            {
                                Value = tagValue,
                                FilterTagId = matchedTagFilter.Id
                            });
                        }
                        await context.FilterTagValues.AddRangeAsync(tagValuesEntity);
                        await context.SaveChangesAsync();
                    }
                }
            }

            List<dynamic> items = new List<dynamic>();
            foreach (var cat in categories)
            {
                // var json = File.ReadAllText($"D:/E-Commerce Store/ProductData/Json_data/{cat}_final_data.json");
                var json = File.ReadAllText(Path.Combine(basePath, $"{cat}.json"));
                var arr = JsonConvert.DeserializeObject<List<dynamic>>(json);
                if (arr != null)
                {
                    items.AddRange(arr);
                }
            }

            //add brands
            logger.LogInformation("Seeding brands...");
            if (!context.Brands.Any())
            {
                var jsonBrands = File.ReadAllText(Path.Combine(basePath, "brands.json"));
                var brandsByCategory = JsonConvert.DeserializeObject<List<BrandGroup>>(jsonBrands);
                if (brandsByCategory == null)
                {
                    logger.LogError("Failed to deserialize brands from JSON file.");
                    throw new Exception("Failed to deserialize brands from JSON file.");
                }
                logger.LogInformation($"Adding {brandsByCategory.Count} brand groups");
                foreach (var brandByCat in brandsByCategory)
                {
                    var matchedCat = categoriesInDb.FirstOrDefault(c => c.Name.ToLower() == brandByCat.category.ToLower() && c.Id == brandByCat.categoryId);
                    if (matchedCat == null)
                    {
                        logger.LogError($"Category '{brandByCat.category}' not found in the database for brand.");
                        throw new Exception($"Category '{brandByCat.category}' not found in the database.");
                    }
                    foreach (var brand in brandByCat.brands)
                    {
                        var brandEntity = new Brand
                        {
                            Name = brand.name,
                            CategoryId = matchedCat.Id,
                            ImageUrl = brand.imageUrl
                        };
                        await context.Brands.AddAsync(brandEntity);
                    }
                }
                await context.SaveChangesAsync();
                logger.LogInformation("Brands seeded.");
            }

            //add product
            logger.LogInformation("Seeding products...");
            if (!context.Products.Any())
            {
                var products = new List<Product>();
                int productCount = 0;
                foreach (var item in items)
                {
                    try
                    {
                        var filterTags = new List<ProductFilterTagValue>();
                        if (item.filter_tags != null)
                        {
                            foreach (var prop in item.filter_tags)
                            {
                                string key = prop.Name;
                                string value = prop.Value.ToString();
                                string itemCategoryLower = ((string)item.category).ToLower();
                                var filterTagValue = await context.FilterTagValues
                                    .Include(ftv => ftv.FilterTag)
                                    .FirstOrDefaultAsync(ftv =>
                                        ftv.FilterTag!.Category!.Name.ToLower() == itemCategoryLower &&
                                        ftv.FilterTag.Name.ToLower() == key.ToLower() &&
                                        ftv.Value.ToLower() == value.ToLower());
                                if (filterTagValue != null)
                                {
                                    var filterTag = new ProductFilterTagValue
                                    {
                                        FilterTagValueId = filterTagValue.Id,
                                    };
                                    filterTags.Add(filterTag);
                                }
                            }
                        }
                        var detailImages = new List<ProductImage>();
                        if (item.imgs != null)
                        {
                            foreach (var img in item.imgs)
                            {
                                detailImages.Add(new ProductImage { ImageUrl = img.url.ToString(), PublicId = img.publicid.ToString() });
                            }
                        }
                        var attributes = new List<ProductAttribute>();
                        if (item.attributes != null)
                        {
                            foreach (var attr in item.attributes)
                            {
                                attributes.Add(new ProductAttribute
                                {
                                    Name = attr.name,
                                    Value = attr.value.ToString(),
                                    DisplayOrder = attr.displayorder,
                                    AttributeType = attr.type.ToString()
                                });
                            }
                        }
                        var displayTags = new List<ProductDisplayTag>();
                        if (item.tags != null)
                        {
                            foreach (var tag in item.tags)
                            {
                                displayTags.Add(new ProductDisplayTag()
                                {
                                    DisplayTag = tag.ToString()
                                });
                            }
                        }
                        // var descriptions = new List<string>();
                        // if (item.descriptions != null)
                        // {
                        //     foreach (var desc in item.descriptions)
                        //     {
                        //         descriptions.Add(desc.ToString());
                        //     }
                        // }
                        var matchedCategory = categoriesInDb.FirstOrDefault(c => c.Name == item.category.ToString());
                        if (matchedCategory == null)
                        {
                            logger.LogError($"Category '{item.category}' not found for product '{item.name}'");
                            throw new Exception($"Category '{item.category}' not found for product '{item.name}'");
                        }
                        string brandName = item.brand.ToString();
                        var brandEntity = context.Brands.FirstOrDefault(b => b.Name == brandName && b.CategoryId == matchedCategory.Id);
                        if (brandEntity == null)
                        {
                            logger.LogError($"Brand '{item.brand}' not found for product '{item.name}'");
                            throw new Exception($"Brand '{item.brand}' not found for product '{item.name}'");
                        }
                        var product = new Product
                        {
                            Name = item.name,
                            Description = item.descriptions,
                            OldPrice = item.old_price,
                            Price = item.price,
                            DiscountPercentage = item.discount,
                            CategoryId = matchedCategory.Id,
                            BrandId = brandEntity.Id,
                            Brand = brandEntity,
                            MainImageUrl = item.image_url,
                            MainImagePublicId = item.image_publicid,
                            QuantityInStock = 1000,
                            ReservedQuantity = 0,
                            UrlSlug = item.urlslug,
                            MetaTitle = item.metatitle,
                            MetaDescription = item.metadescription,
                            MetaKeywords = item.metakeywords,
                            DisplayTags = displayTags,
                            ProductFilterTagValues = filterTags,
                            // Reviews = new List<Review>(),
                            DetailImages = detailImages,
                            Attributes = attributes,
                        };
                        products.Add(product);
                        productCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error seeding product: {item.name}");
                    }
                }
                await context.Products.AddRangeAsync(products);
                logger.LogInformation($"Seeded {productCount} products.");
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed.");
        }
    }
}