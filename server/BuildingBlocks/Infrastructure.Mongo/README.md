# Infrastructure.Mongo

Base repository implementation for MongoDB using **MongoDB.Entities** library.

## 📦 Package Dependencies

- `MongoDB.Entities` v25.0.0
- `Shared.Core` (project reference)

## 🏗️ Architecture

Tương tự `Infrastructure.EF` nhưng cho MongoDB:

```
Infrastructure.Mongo/
├── Entities/
│   └── MongoEntity.cs          # Base entity cho MongoDB
└── Repositories/
    └── BaseMongoRepository.cs  # Implementation của IBaseRepository<T, TId>
```

## 🚀 Cách sử dụng

### 1. Entity (trong SearchService)

```csharp
using Infrastructure.Mongo.Entities;

namespace SearchService.Entities
{
    public class Item : MongoEntity  // Kế thừa từ MongoEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        // ... other properties
    }
}
```

### 2. Repository Interface (trong SearchService)

```csharp
using Shared.Core.Domain.Repositories;

namespace SearchService.Repositories
{
    public interface IItemRepository : IBaseRepository<Item, string>
    {
        // Custom methods specific to Item
        Task<IEnumerable<Item>> SearchByNameAsync(string searchTerm);
    }
}
```

### 3. Repository Implementation (trong SearchService)

```csharp
using Infrastructure.Mongo.Repositories;
using MongoDB.Entities;

namespace SearchService.Repositories
{
    public class ItemRepository : BaseMongoRepository<Item, string>, IItemRepository
    {
        public async Task<IEnumerable<Item>> SearchByNameAsync(string searchTerm)
        {
            return await DB.Find<Item>()
                .Match(x => x.Name.Contains(searchTerm))
                .ExecuteAsync();
        }
    }
}
```

### 4. Registration & Initialization (Program.cs)

```csharp
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Initialize MongoDB
await DB.InitAsync("SearchDb", "mongodb://localhost:27017");

// Register repositories
builder.Services.AddScoped<IItemRepository, ItemRepository>();

var app = builder.Build();
app.Run();
```

## 🆚 So sánh với Infrastructure.EF

| Feature | Infrastructure.EF | Infrastructure.Mongo |
|---------|------------------|---------------------|
| **Base Entity** | `IEntity<TId>` | `MongoEntity` (kế thừa `Entity`) |
| **ID Type** | Generic `TId` | String (MongoDB ObjectId) |
| **Context** | `DbContext` | `DB` static class |
| **Transactions** | `DbContext.SaveChanges()` | `DB.Transaction()` |
| **Include/Join** | `Include()` | `Populate()` hoặc `Project()` |

## ⚠️ Lưu ý quan trọng

### MongoDB.Entities vs EF Core

1. **Async-first**: MongoDB.Entities chủ yếu async, không có sync methods
   - `Update(entity)` và `Delete(entity)` trong interface là fire-and-forget
   - Nên dùng `UpdateAsync()` và `DeleteAsync()` thay thế

2. **No DbContext**: Không cần inject DbContext
   - Dùng static `DB` class
   - Gọi `DB.InitAsync()` ở Program.cs

3. **ID luôn là string**: MongoDB dùng ObjectId (string 24 chars)
   - Entity class nên dùng `MongoEntity` (Id: string)
   - Hoặc override `Id` property nếu cần custom type

4. **No Include pattern**: MongoDB không có eager loading như EF
   - Dùng `DB.Find<T>().Match().Project()` để select fields
   - Hoặc override `GetByIdAsync` để populate relationships

## 📝 Example: SearchService Migration

**Before (using EF Core pattern):**
```csharp
public class Item : BaseEntity
{
    public int Id { get; set; }
    // ...
}
```

**After (using MongoDB pattern):**
```csharp
public class Item : MongoEntity
{
    // ID property inherited from MongoEntity (string type)
    // ...
}
```

## 🔗 Related

- [MongoDB.Entities Documentation](https://mongodb-entities.com/)
- [Infrastructure.EF](../Infrastructure.EF/README.md)
- [Shared.Core](../Shared.Core/README.md)
