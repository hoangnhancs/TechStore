# BuildingBlocks - Clean Architecture Infrastructure

Tập hợp các Building Blocks theo DDD principles cho TechStore microservices.

## 📦 Cấu trúc

### **SharedCore** - Domain Abstractions
Pure domain layer, không phụ thuộc vào infrastructure hoặc framework nào.

**Bao gồm:**
- `IEntity<TId>` - Base entity interface
- `BaseEntity<TId>` - Entity với audit fields (CreatedAt, UpdatedAt, etc.)
- `AggregateRoot<TId>` - Aggregate root với domain events
- `IBaseRepository<T, TId>` - Repository interface không phụ thuộc EF Core
- `IUnitOfWork` - Unit of Work interface (KHÔNG expose DbContext)
- `IDomainEvent` - Domain event interface

### **Infrastructure.EF** - EF Core Implementation
Generic implementation cho Entity Framework Core, có thể tái sử dụng cho mọi DbContext.

**Bao gồm:**
- `BaseRepository<T, TId, TContext>` - Generic repository cho bất kỳ DbContext nào
- `UnitOfWork<TContext>` - Generic Unit of Work với transaction support
- `ServiceCollectionExtensions` - DI registration helpers

## 🚀 Cách sử dụng

### 1️⃣ Trong ProductService

#### **Update Entity**
```csharp
using SharedCore.Domain.Entities;

// Option 1: Sử dụng BaseEntity (có audit fields)
public class Product : BaseEntity<string>
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    // ...
}

// Option 2: Sử dụng AggregateRoot (có domain events)
public class Product : AggregateRoot<string>
{
    public required string Name { get; set; }
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        AddDomainEvent(new ProductPriceChanged(Id, newPrice));
    }
}
```

#### **Update Repository**
```csharp
using Infrastructure.EF.Repositories;
using SharedCore.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product, string>
{
    Task<List<Product>> GetTop10ProductPerCategory(CancellationToken ct);
}

public class ProductRepository 
    : BaseRepository<Product, string, ProductSvcDbContext>, IProductRepository
{
    public ProductRepository(ProductSvcDbContext context) : base(context) { }

    // Custom methods
    public async Task<List<Product>> GetTop10ProductPerCategory(CancellationToken ct)
    {
        return await DbSet
            .FromSqlRaw("...")
            .ToListAsync(ct);
    }
}
```

#### **Update Unit of Work**
```csharp
using SharedCore.UnitOfWork;
using Infrastructure.EF.UnitOfWork;

// Option 1: Sử dụng generic UnitOfWork (không cần custom interface)
// Chỉ inject IUnitOfWork và các repository riêng lẻ

// Option 2: Custom UnitOfWork nếu cần
public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    // KHÔNG expose Context!
}

public class ProductUnitOfWork : UnitOfWork<ProductSvcDbContext>, IProductUnitOfWork
{
    private readonly Lazy<IProductRepository> _productRepository;

    public ProductUnitOfWork(ProductSvcDbContext context) : base(context)
    {
        _productRepository = new Lazy<IProductRepository>(
            () => new ProductRepository(Context));
    }

    public IProductRepository ProductRepository => _productRepository.Value;
}
```

#### **Update Program.cs**
```csharp
using Infrastructure.EF.Extensions;

// Register DbContext
builder.Services.AddDbContext<ProductSvcDbContext>(options =>
    options.UseNpgsql(connectionString));

// Option 1: Generic UnitOfWork (recommended cho đơn giản)
builder.Services.AddEFInfrastructure<ProductSvcDbContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Option 2: Custom UnitOfWork
builder.Services.AddEFInfrastructure<ProductSvcDbContext, ProductUnitOfWork>();
builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
```

#### **Sử dụng trong Service/Controller**
```csharp
public class ProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepo,
        IUnitOfWork unitOfWork)
    {
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> CreateProduct(CreateProductDto dto)
    {
        var product = new Product { ... };
        
        await _productRepo.AddAsync(product);
        await _unitOfWork.CommitAsync();
        
        return product;
    }

    // Với Transaction
    public async Task UpdateProductWithImages(string id, UpdateDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var product = await _productRepo.GetByIdAsync(id);
            product.Update(dto);
            
            _productRepo.Update(product);
            // Other operations...
            
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
```

## ✅ Lợi ích

### **Tách biệt Domain và Infrastructure**
- Domain không phụ thuộc EF Core, Npgsql, hay bất kỳ framework nào
- Có thể test domain logic mà không cần database
- Có thể chuyển đổi ORM dễ dàng (EF Core → Dapper → MongoDB)

### **Không expose DbContext**
- Services chỉ làm việc với repositories và UnitOfWork
- Không thể bypass repository để truy cập trực tiếp Context
- Tuân thủ nguyên tắc DDD

### **Reusable**
- `BaseRepository` và `UnitOfWork` dùng được cho mọi service
- Không duplicate code
- Dễ maintain và test

### **Transaction Support**
- `BeginTransactionAsync()` / `CommitTransactionAsync()` / `RollbackAsync()`
- Xử lý distributed transaction với outbox pattern

## 🔜 Tương lai: Infrastructure.Mongo

Khi cần MongoDB cho services khác (như SearchService):

```csharp
// Infrastructure.Mongo/Repositories/MongoRepository.cs
public class MongoRepository<T, TId> : IBaseRepository<T, TId>
{
    private readonly IMongoCollection<T> _collection;
    // Implement với MongoDB.Driver
}

// SearchService/Program.cs
builder.Services.AddMongoInfrastructure<SearchDbContext>();
```

## 📝 Migration Guide

### Bước 1: Update Dependencies
```xml
<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\SharedCore\Shared.Core.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\Infrastructure.EF\Infrastructure.EF.csproj" />
</ItemGroup>
```

### Bước 2: Update Entities
Kế thừa từ `BaseEntity<TId>` hoặc `AggregateRoot<TId>`

### Bước 3: Update Repositories
Extend từ `BaseRepository<T, TId, TContext>`

### Bước 4: Update Unit of Work
Không expose Context, chỉ expose repositories

### Bước 5: Update DI Registration
Sử dụng `AddEFInfrastructure<TContext>()`

### Bước 6: Remove Old Code
Xóa các file cũ: old IUnitOfWork, old BaseRepository
