# Migration Guide - Refactoring to Clean Architecture

Guide chi tiết để migrate ProductService từ cấu trúc cũ sang BuildingBlocks mới.

## 📋 Checklist

- [ ] **Bước 1**: Add project references
- [ ] **Bước 2**: Update Entity classes  
- [ ] **Bước 3**: Update Repository implementations
- [ ] **Bước 4**: Update Unit of Work
- [ ] **Bước 5**: Update DI registration (Program.cs)
- [ ] **Bước 6**: Update Controllers/Services
- [ ] **Bước 7**: Remove old files
- [ ] **Bước 8**: Test

---

## 📝 Bước 1: Add Project References

### Cập nhật `ProductService.csproj`:

```xml
<ItemGroup>
  <!-- Thêm references đến BuildingBlocks -->
  <ProjectReference Include="..\..\BuildingBlocks\SharedCore\Shared.Core.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\Infrastructure.EF\Infrastructure.EF.csproj" />
  
  <!-- Giữ nguyên references cũ -->
  <ProjectReference Include="..\Contract\Contract.csproj" />
  <ProjectReference Include="..\PhotoService\PhotoService.csproj" />
  <ProjectReference Include="..\shared-web\Shared.Web.csproj" />
</ItemGroup>
```

### Có thể remove (sau khi migrate xong):
```xml
<!-- Có thể remove sau -->
<ProjectReference Include="..\shared-core\src\Shared.Core\Shared.Core.csproj" />
```

---

## 📝 Bước 2: Update Entity Classes

### Trước (Product.cs):
```csharp
using Shared.Core.Domain.Interface;

public class Product : IEntity<string>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // ... properties
}
```

### Sau (Product.cs):
```csharp
using SharedCore.Domain.Entities;

public class Product : BaseEntity<string>
{
    // Id, CreatedAt, UpdatedAt đã có từ BaseEntity - REMOVE chúng!
    
    public required string Name { get; set; }
    // ... other properties

    public Product() : base()
    {
        Id = Guid.NewGuid().ToString();
    }

    // Add domain methods
    public void UpdatePrice(decimal newPrice, decimal oldPrice, decimal discount)
    {
        Price = newPrice;
        OldPrice = oldPrice;
        DiscountPercentage = discount;
        SetUpdatedAt(); // Từ BaseEntity
    }
}
```

**Quan trọng:**
- ❌ REMOVE properties: `Id`, `CreatedAt`, `UpdatedAt` (đã có trong `BaseEntity`)
- ✅ Thêm constructor gọi `base()`
- ✅ Thêm domain methods (business logic)

**Apply cho các entities khác:**
- `Category`, `Brand`, `ProductImage`, etc. - tất cả extend từ `BaseEntity<TId>`

---

## 📝 Bước 3: Update Repository Implementations

### Trước (ProductRepository.cs):
```csharp
using ProductService.Data;
using Shared.Core.Domain.Interface;

public class ProductRepository : BaseRepository<Product, string>, IProductRepository
{
    public ProductRepository(ProductSvcDbContext context) : base(context) { }
    
    // Custom methods...
}
```

### Sau (ProductRepository.cs):
```csharp
using Infrastructure.EF.Repositories;
using ProductService.Data;
using SharedCore.Domain.Repositories;

public class ProductRepository 
    : BaseRepository<Product, string, ProductSvcDbContext>, IProductRepository
{
    public ProductRepository(ProductSvcDbContext context) : base(context) { }
    
    // Custom methods - sử dụng DbSet từ base class
    public async Task<List<Product>> GetTop10ProductPerCategory(CancellationToken ct)
    {
        return await DbSet  // DbSet từ BaseRepository
            .FromSqlRaw("...")
            .Include(p => p.Category)
            .ToListAsync(ct);
    }
}
```

**Thay đổi:**
- ✅ Extend từ `BaseRepository<Product, string, ProductSvcDbContext>` (3 type params)
- ✅ Import từ `Infrastructure.EF.Repositories`
- ✅ Update interface import từ `SharedCore.Domain.Repositories`

**Apply cho:**
- `CategoryRepository`, `BrandRepository`, etc.

### Update Repository Interface (IProductRepository.cs):
```csharp
using SharedCore.Domain.Repositories;
using ProductService.Entities;

public interface IProductRepository : IBaseRepository<Product, string>
{
    // Chỉ custom methods
    Task<List<Product>> GetTop10ProductPerCategory(CancellationToken ct);
}
```

---

## 📝 Bước 4: Update Unit of Work

### 4.1. Update Interface (IProductUnitOfWork.cs):

**Trước:**
```csharp
public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    ProductSvcDbContext Context { get; }  // ❌ VI PHẠM DDD!
}
```

**Sau:**
```csharp
using SharedCore.UnitOfWork;
using ProductService.Repositories.Interface;

public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    // ✅ KHÔNG expose Context
}
```

### 4.2. Update Implementation (ProductUnitOfWork.cs):

**Trước:**
```csharp
public class ProductUnitOfWork : IProductUnitOfWork
{
    private readonly ProductSvcDbContext _context;
    private IProductRepository? _productRepository;
    
    public IProductRepository ProductRepository => 
        _productRepository ??= new ProductRepository(_context);
    
    public ProductSvcDbContext Context => _context;  // ❌
    
    public ProductUnitOfWork(ProductSvcDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> CommitAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
    
    // Dispose, Repository<T, TId>, etc.
}
```

**Sau:**
```csharp
using Infrastructure.EF.UnitOfWork;
using ProductService.Data;
using ProductService.Repositories;
using ProductService.Repositories.Interface;

public class ProductUnitOfWork : UnitOfWork<ProductSvcDbContext>, IProductUnitOfWork
{
    private readonly Lazy<IProductRepository> _productRepository;

    public ProductUnitOfWork(ProductSvcDbContext context) : base(context)
    {
        _productRepository = new Lazy<IProductRepository>(
            () => new ProductRepository(Context));
    }

    public IProductRepository ProductRepository => _productRepository.Value;
    
    // ✅ CommitAsync, Dispose, BeginTransaction, etc. từ base class
}
```

**Thay đổi:**
- ✅ Extend từ `UnitOfWork<ProductSvcDbContext>`
- ✅ Sử dụng `Lazy<T>` cho repositories (khởi tạo khi cần)
- ✅ REMOVE `Context` property exposure
- ✅ REMOVE `CommitAsync`, `Dispose` implementations (đã có trong base)
- ✅ Bonus: Có thêm transaction support (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackAsync`)

---

## 📝 Bước 5: Update DI Registration (Program.cs)

**Trước:**
```csharp
// Old registration
builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
// Không có BaseRepository registration
```

**Sau - OPTION 1** (Recommended - đơn giản hơn):
```csharp
using Infrastructure.EF.Extensions;

// DbContext
builder.Services.AddDbContext<ProductSvcDbContext>(options =>
    options.UseNpgsql(connectionString));

// Infrastructure
builder.Services.AddEFInfrastructure<ProductSvcDbContext>();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// ... other repositories
```

**Sau - OPTION 2** (Custom UnitOfWork):
```csharp
using Infrastructure.EF.Extensions;

// DbContext
builder.Services.AddDbContext<ProductSvcDbContext>(options =>
    options.UseNpgsql(connectionString));

// Infrastructure with custom UnitOfWork
builder.Services.AddEFInfrastructure<ProductSvcDbContext, ProductUnitOfWork>();
builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();

// Repositories (nếu cần inject trực tiếp)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

**So sánh:**
| | Option 1 (Generic) | Option 2 (Custom) |
|---|---|---|
| **Đơn giản** | ✅ | ❌ |
| **Inject** | `IUnitOfWork + IProductRepository` | `IProductUnitOfWork` |
| **Use case** | Đa số trường hợp | Khi cần group repositories |

---

## 📝 Bước 6: Update Controllers/Services

### Trước:
```csharp
public class ProductService
{
    private readonly IProductUnitOfWork _unitOfWork;
    
    public async Task CreateProduct(...)
    {
        var product = new Product { ... };
        await _unitOfWork.ProductRepository.AddAsync(product);
        
        // ❌ Có thể bypass repository
        _unitOfWork.Context.Products.Add(product);
        
        await _unitOfWork.CommitAsync();
    }
}
```

### Sau - Option 1 (Generic UnitOfWork):
```csharp
using SharedCore.UnitOfWork;
using ProductService.Repositories.Interface;

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
    
    public async Task CreateProduct(...)
    {
        var product = new Product { Name = "..." };
        
        await _productRepo.AddAsync(product);
        await _unitOfWork.CommitAsync();
    }
    
    // With transaction
    public async Task UpdateWithTransaction(...)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Multiple operations
            await _productRepo.AddAsync(...);
            // ...
            
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

### Sau - Option 2 (Custom UnitOfWork):
```csharp
public class ProductService
{
    private readonly IProductUnitOfWork _unitOfWork;
    
    public ProductService(IProductUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task CreateProduct(...)
    {
        var product = new Product { Name = "..." };
        
        await _unitOfWork.ProductRepository.AddAsync(product);
        await _unitOfWork.CommitAsync();
        
        // ✅ Không thể access Context nữa!
    }
}
```

**Thay đổi:**
- ✅ KHÔNG access được `Context` nữa
- ✅ Sử dụng domain methods thay vì set properties trực tiếp
- ✅ Transaction support built-in

---

## 📝 Bước 7: Remove Old Files

Sau khi test xong, có thể xóa:

### Trong ProductService:
```
❌ Repositories/BaseRepository.cs (dùng từ Infrastructure.EF)
❌ Old implementations có expose Context
```

### Trong shared-core (nếu không còn service nào dùng):
```
❌ server/shared-core/src/Shared.Core/Domain/Interface/IUnitOfWork.cs
❌ server/shared-core/src/Shared.Core/Domain/Interface/IBaseRepository.cs
❌ server/shared-core/src/Shared.Core/Domain/Interface/IEntity.cs
```

**⚠️ Chú ý:** Chỉ xóa khi:
- ✅ Tất cả services đã migrate (ProductService, IdentityService, SearchService)
- ✅ Không còn reference nào đến old files

---

## 📝 Bước 8: Testing

### 8.1. Build
```bash
cd server/ProductService
dotnet build
```

### 8.2. Run Tests
```bash
dotnet test
```

### 8.3. Manual Testing
- ✅ CRUD operations
- ✅ Custom repository methods
- ✅ Transaction scenarios
- ✅ Concurrent requests

---

## 🔄 Apply cho IdentityService

Làm tương tự:

1. **Add references** to BuildingBlocks
2. **Update entities**: `User : BaseEntity<string>`
3. **Update repositories**: `UserRepository : BaseRepository<User, string, IdentityDbContext>`
4. **Update UnitOfWork**: `IdentityUnitOfWork : UnitOfWork<IdentityDbContext>`
5. **Update Program.cs**:
   ```csharp
   builder.Services.AddEFInfrastructure<IdentityDbContext, IdentityUnitOfWork>();
   ```

---

## 🔮 Future: SearchService với MongoDB

```csharp
// BuildingBlocks/Infrastructure.Mongo/
public class MongoRepository<T, TId> : IBaseRepository<T, TId>
{
    private readonly IMongoCollection<T> _collection;
    // Implement với MongoDB.Driver
}

// SearchService/Program.cs
builder.Services.AddMongoInfrastructure<SearchDbContext>();
```

---

## ❓ FAQ

**Q: Có cần migrate tất cả services cùng lúc không?**
A: Không. Có thể migrate từng service một. BuildingBlocks và old shared-core có thể coexist.

**Q: Làm sao để test mà không ảnh hưởng production?**
A: 
1. Tạo branch mới
2. Keep old files với suffix `.Old.cs`
3. Test thoroughly
4. Remove old files khi đã ổn định

**Q: Interface có quá nhiều dependencies phải inject?**
A: Sử dụng Option 2 (Custom UnitOfWork) để group repositories.

**Q: Khi nào dùng AggregateRoot thay vì BaseEntity?**
A: Khi entity cần raise Domain Events (ví dụ: Product updated → notify SearchService).

---

## 📚 Reference

- [README.md](./README.md) - Tổng quan về BuildingBlocks
- [Product.Refactored.cs](../ProductService/Entities/Product.Refactored.cs) - Entity example
- [ProductRepository.Refactored.cs](../ProductService/Repositories/ProductRepository.Refactored.cs) - Repository example
- [ProductUnitOfWork.Refactored.cs](../ProductService/Persistence/ProductUnitOfWork.Refactored.cs) - UnitOfWork example
- [ProductsController.Refactored.Example.cs](../ProductService/Controllers/ProductsController.Refactored.Example.cs) - Controller example
