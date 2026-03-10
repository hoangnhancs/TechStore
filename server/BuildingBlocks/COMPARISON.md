# So sánh: Trước và Sau Refactoring

## 🔴 VẤN ĐỀ CŨ (Before)

### 1. **Expose DbContext ra ngoài - Vi phạm DDD**

```csharp
public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    ProductSvcDbContext Context { get; }  // ❌ EXPOSE CONTEXT
}

// Service có thể bypass repository
public class ProductService
{
    public async Task DoSomething()
    {
        // ❌ Bypass repository - truy cập trực tiếp Context
        var products = await _unitOfWork.Context.Products
            .Where(p => p.IsActive)
            .ToListAsync();
            
        // ❌ Không thể test vì phụ thuộc infrastructure
    }
}
```

### 2. **BaseRepository coupled với DbContext cụ thể**

```csharp
// Trong ProductService/Repositories/BaseRepository.cs
public class BaseRepository<T, TId> : IBaseRepository<T, TId>
{
    protected readonly ProductSvcDbContext _context;  // ❌ HARD-CODED
    
    public BaseRepository(ProductSvcDbContext context) { }
}

// Trong IdentityService/Repositories/BaseRepository.cs
public class BaseRepository<T, TId> : IBaseRepository<T, TId>
{
    protected readonly IdentityDbContext _context;  // ❌ DUPLICATE CODE
    
    public BaseRepository(IdentityDbContext context) { }
}
```

**Vấn đề:**
- ❌ Duplicate code ở mỗi service
- ❌ Không reusable
- ❌ Tight coupling với EF Core

### 3. **Entity không có Business Logic**

```csharp
public class Product : IEntity<string>
{
    public string Id { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    // ... chỉ có properties, không có methods
}

// Business logic scattered across services
public class ProductService
{
    public async Task UpdateStock(string id, int quantity)
    {
        var product = await _repo.GetByIdAsync(id);
        
        // ❌ Logic nằm ngoài entity (Anemic Domain Model)
        if (product.Stock < quantity)
            throw new Exception("Not enough stock");
            
        product.Stock -= quantity;
        product.UpdatedAt = DateTime.UtcNow;  // ❌ Dễ quên update
    }
}
```

### 4. **Không có Domain Events**

```csharp
public async Task DecreaseStock(string productId, int qty)
{
    var product = await _productRepo.GetByIdAsync(productId);
    product.Stock -= qty;
    
    await _productRepo.UpdateAsync(product);
    
    // ❌ Phải manually notify các services khác
    await _messageBus.PublishAsync(new ProductStockChangedEvent(...));
}
```

---

## ✅ GIẢI PHÁP MỚI (After)

### 1. **Không expose Context - Tuân thủ DDD**

```csharp
// SharedCore/UnitOfWork/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    Task<bool> CommitAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    
    // ✅ KHÔNG có Context property
}

// ProductService specific
public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    // ✅ Chỉ expose repositories, không expose Context
}

// Service KHÔNG thể bypass repository
public class ProductService
{
    private readonly IProductUnitOfWork _unitOfWork;
    
    public async Task DoSomething()
    {
        // ✅ Phải dùng repository - không access được Context
        var products = await _unitOfWork.ProductRepository.GetListAsync(
            predicate: p => p.IsActive
        );
        
        // ✅ Có thể mock IProductRepository để test
    }
}
```

### 2. **Generic BaseRepository - Reusable**

```csharp
// Infrastructure.EF/Repositories/BaseRepository.cs
public class BaseRepository<T, TId, TContext> : IBaseRepository<T, TId>
    where T : class, IEntity<TId>
    where TContext : DbContext  // ✅ GENERIC - works with ANY DbContext
{
    protected readonly TContext Context;  // ✅ Generic
    protected readonly DbSet<T> DbSet;
    
    public BaseRepository(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }
    
    // Implementation...
}

// ProductService - REUSE
public class ProductRepository 
    : BaseRepository<Product, string, ProductSvcDbContext>, IProductRepository
{
    // ✅ Chỉ implement custom methods
}

// IdentityService - REUSE
public class UserRepository 
    : BaseRepository<User, string, IdentityDbContext>, IUserRepository
{
    // ✅ CÙNG code, khác DbContext
}
```

**Lợi ích:**
- ✅ Zero duplication
- ✅ Reusable across services
- ✅ Easy to switch ORM (can create Infrastructure.Dapper)

### 3. **Rich Domain Model với Business Logic**

```csharp
// SharedCore/Domain/Entities/BaseEntity.cs
public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    
    public void SetUpdatedAt(DateTime? updatedAt = null)
    {
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }
}

// ProductService/Entities/Product.cs
public class Product : BaseEntity<string>
{
    public decimal Price { get; private set; }  // ✅ private setter
    public int Stock { get; private set; }
    
    // ✅ Domain methods encapsulate business logic
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive");
            
        Price = newPrice;
        SetUpdatedAt();  // ✅ Auto update timestamp
    }
    
    public void DecreaseStock(int quantity)
    {
        if (Stock < quantity)
            throw new InvalidOperationException("Not enough stock");
            
        Stock -= quantity;
        SetUpdatedAt();  // ✅ Không quên update
    }
}

// Service code becomes cleaner
public class ProductService
{
    public async Task UpdateStock(string id, int quantity)
    {
        var product = await _repo.GetByIdAsync(id);
        
        // ✅ Logic trong entity - Rich Domain Model
        product.DecreaseStock(quantity);  // Validates & updates automatically
        
        _repo.Update(product);
        await _unitOfWork.CommitAsync();
    }
}
```

### 4. **Domain Events Support**

```csharp
// SharedCore/Domain/Entities/AggregateRoot.cs
public abstract class AggregateRoot<TId> : BaseEntity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// ProductService/Entities/Product.cs
public class Product : AggregateRoot<string>
{
    public void DecreaseStock(int quantity, string orderId)
    {
        if (Stock < quantity)
            throw new InvalidOperationException("Not enough stock");
            
        Stock -= quantity;
        SetUpdatedAt();
        
        // ✅ Auto raise domain event
        AddDomainEvent(new ProductStockChangedEvent(Id, Stock, orderId));
    }
}

// Domain event handler can dispatch to message bus
public class ProductStockChangedEventHandler : INotificationHandler<ProductStockChangedEvent>
{
    public async Task Handle(ProductStockChangedEvent evt, CancellationToken ct)
    {
        // ✅ Auto dispatch to RabbitMQ/Kafka
        await _messageBus.PublishAsync(evt);
    }
}
```

---

## 📊 So sánh tổng quan

| Khía cạnh | ❌ Before | ✅ After |
|-----------|----------|---------|
| **Expose Context** | ✅ Có | ❌ Không |
| **DDD Compliance** | ❌ Vi phạm | ✅ Tuân thủ |
| **Code Duplication** | ❌ BaseRepository ở mỗi service | ✅ Shared Infrastructure |
| **Testability** | ❌ Khó test (phụ thuộc infrastructure) | ✅ Dễ test (mock repositories) |
| **Domain Logic** | ❌ Anemic model (logic ngoài entity) | ✅ Rich model (logic trong entity) |
| **Transaction Support** | ❌ Phải tự implement | ✅ Built-in |
| **Domain Events** | ❌ Không có | ✅ Có support |
| **Flexibility** | ❌ Tied to EF Core | ✅ Can switch ORM |
| **Reusability** | ❌ Mỗi service tự implement | ✅ Reuse BuildingBlocks |

---

## 🎯 Kết luận

### Trước Refactoring:
```
❌ Anemic Domain Model
❌ Infrastructure Leakage (expose Context)
❌ Code Duplication
❌ Hard to Test
❌ Tight Coupling
```

### Sau Refactoring:
```
✅ Rich Domain Model
✅ Clean Separation of Concerns
✅ Zero Duplication
✅ Highly Testable
✅ Loose Coupling
✅ DDD Compliant
✅ Transaction Support
✅ Domain Events
✅ Reusable Infrastructure
```

---

## 📁 Cấu trúc thư mục

### Before:
```
server/
├── shared-core/              ← Interface definitions
│   └── Domain/Interface/
│       ├── IEntity.cs
│       ├── IBaseRepository.cs
│       └── IUnitOfWork.cs
│
├── ProductService/
│   ├── Repositories/
│   │   └── BaseRepository.cs     ❌ Duplicate code
│   └── Persistence/
│       ├── IProductUnitOfWork.cs  ❌ Expose Context
│       └── ProductUnitOfWork.cs
│
└── IdentityService/
    ├── Repositories/
    │   └── BaseRepository.cs     ❌ Duplicate code (again!)
    └── Persistence/
        ├── IIdentityUnitOfWork.cs ❌ Expose Context
        └── IdentityUnitOfWork.cs
```

### After:
```
server/
├── BuildingBlocks/
│   ├── SharedCore/              ✅ Pure domain abstractions
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── IEntity.cs
│   │   │   │   ├── BaseEntity.cs
│   │   │   │   └── AggregateRoot.cs
│   │   │   └── Repositories/
│   │   │       └── IBaseRepository.cs
│   │   └── UnitOfWork/
│   │       └── IUnitOfWork.cs
│   │
│   └── Infrastructure.EF/       ✅ Generic, reusable implementation
│       ├── Repositories/
│       │   └── BaseRepository.cs    ✅ Generic for ANY DbContext
│       ├── UnitOfWork/
│       │   └── UnitOfWork.cs        ✅ Generic for ANY DbContext
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
│
├── ProductService/
│   ├── Entities/
│   │   └── Product.cs            ✅ Extends BaseEntity
│   ├── Repositories/
│   │   └── ProductRepository.cs  ✅ Extends BaseRepository<T,TId,TContext>
│   └── Persistence/
│       ├── IProductUnitOfWork.cs ✅ NO Context exposure
│       └── ProductUnitOfWork.cs  ✅ Extends UnitOfWork<TContext>
│
└── IdentityService/              ✅ Same pattern, zero duplication
    ├── Entities/
    │   └── User.cs               ✅ Extends BaseEntity
    ├── Repositories/
    │   └── UserRepository.cs     ✅ Extends BaseRepository<T,TId,TContext>
    └── Persistence/
        ├── IIdentityUnitOfWork.cs
        └── IdentityUnitOfWork.cs ✅ Extends UnitOfWork<TContext>
```

---

## 💡 Key Takeaways

1. **DDD Principle**: Domain layer không phụ thuộc infrastructure
2. **SoC (Separation of Concerns)**: Tách biệt rõ ràng domain và infrastructure
3. **DRY (Don't Repeat Yourself)**: Reuse BuildingBlocks across services
4. **SOLID**: Tuân thủ các nguyên tắc OOP
5. **Testability**: Dễ test nhờ abstractions và dependency injection
6. **Flexibility**: Có thể thay đổi infrastructure (EF Core → MongoDB) mà không ảnh hưởng domain

