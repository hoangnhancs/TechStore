# Quick Start Guide - Apply Refactoring

## 🚀 Bắt đầu nhanh cho ProductService

### Bước 1: Build BuildingBlocks

```powershell
# Build SharedCore
cd server/BuildingBlocks/SharedCore
dotnet build

# Build Infrastructure.EF
cd ../Infrastructure.EF
dotnet build
```

### Bước 2: Update ProductService.csproj

Thêm vào `<ItemGroup>`:

```xml
<ProjectReference Include="..\..\BuildingBlocks\SharedCore\Shared.Core.csproj" />
<ProjectReference Include="..\..\BuildingBlocks\Infrastructure.EF\Infrastructure.EF.csproj" />
```

### Bước 3: Backup files hiện tại

```powershell
cd server/ProductService

# Backup
cp Entities/Product.cs Entities/Product.Old.cs
cp Repositories/BaseRepository.cs Repositories/BaseRepository.Old.cs
cp Repositories/ProductRepository.cs Repositories/ProductRepository.Old.cs
cp Persistence/IProductUnitOfWork.cs Persistence/IProductUnitOfWork.Old.cs
cp Persistence/ProductUnitOfWork.cs Persistence/ProductUnitOfWork.Old.cs
cp Program.cs Program.Old.cs
```

### Bước 4: Replace với files mới

```powershell
# Copy refactored files
cp Entities/Product.Refactored.cs Entities/Product.cs
cp Repositories/ProductRepository.Refactored.cs Repositories/ProductRepository.cs
cp Persistence/IProductUnitOfWork.Refactored.cs Persistence/IProductUnitOfWork.cs
cp Persistence/ProductUnitOfWork.Refactored.cs Persistence/ProductUnitOfWork.cs
```

### Bước 5: Update Program.cs

Thêm:
```csharp
using Infrastructure.EF.Extensions;

// Replace
// builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();

// With
builder.Services.AddEFInfrastructure<ProductSvcDbContext, ProductUnitOfWork>();
builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### Bước 6: Build & Test

```powershell
dotnet build
dotnet test
dotnet run
```

### Bước 7: Verify

✅ Checklist:
- [ ] Build thành công
- [ ] Tests pass
- [ ] API endpoints hoạt động
- [ ] CRUD operations work
- [ ] Custom queries work (GetTop10ProductPerCategory)
- [ ] Transaction scenarios work

### Bước 8: Cleanup (sau khi test xong)

```powershell
# Remove old files
rm Entities/Product.Old.cs
rm Repositories/BaseRepository.cs
rm Repositories/BaseRepository.Old.cs
rm Repositories/ProductRepository.Old.cs
rm Persistence/IProductUnitOfWork.Old.cs
rm Persistence/ProductUnitOfWork.Old.cs
rm Program.Old.cs

# Remove refactored example files
rm Entities/Product.Refactored.cs
rm Entities/Product.WithEvents.Example.cs
rm Repositories/ProductRepository.Refactored.cs
rm Persistence/IProductUnitOfWork.Refactored.cs
rm Persistence/ProductUnitOfWork.Refactored.cs
rm Controllers/ProductsController.Refactored.Example.cs
rm Program.cs.Refactored.Example
```

---

## 🔄 Apply cho các services khác

### IdentityService:
```powershell
cd server/IdentityService

# 1. Update .csproj (add BuildingBlocks references)
# 2. Update Entities/User.cs -> BaseEntity<string>
# 3. Update Repositories/UserRepository.cs -> BaseRepository<User, string, IdentityDbContext>
# 4. Update Persistence/IdentityUnitOfWork.cs -> UnitOfWork<IdentityDbContext>
# 5. Update Program.cs -> AddEFInfrastructure<IdentityDbContext>()

dotnet build
dotnet test
```

### SearchService (nếu dùng EF):
```powershell
cd server/SearchService

# Same steps as above
# OR if using MongoDB -> wait for Infrastructure.Mongo
```

---

## 📚 Documents

Đọc thêm các tài liệu chi tiết:

1. **[README.md](./README.md)** - Tổng quan về BuildingBlocks
2. **[COMPARISON.md](./COMPARISON.md)** - So sánh Before/After
3. **[MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md)** - Hướng dẫn chi tiết từng bước

---

## 🆘 Troubleshooting

### Build errors: "Type not found"
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### "Context is protected"
Đúng rồi! Giờ không expose Context ra ngoài nữa.
✅ Sử dụng repositories thay vì truy cập Context trực tiếp.

### "Cannot access BaseEntity properties"
Kiểm tra:
- ✅ Entity có extend từ `BaseEntity<TId>` không?
- ✅ Import đúng namespace: `using SharedCore.Domain.Entities;`

### Tests fail
- ✅ Update test mocks từ old interfaces sang new interfaces
- ✅ Không mock Context nữa, mock repositories

---

## 💬 Feedback

Nếu gặp vấn đề hoặc có câu hỏi, vui lòng:
1. Check [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) - FAQ section
2. Review example files: `*.Refactored.cs`, `*.Example.cs`
3. Compare với [COMPARISON.md](./COMPARISON.md)

---

## ✅ Success Criteria

Khi refactoring hoàn tất:

- ✅ **No Context Exposure**: Không còn `Context` property trong UnitOfWork interface
- ✅ **Rich Domain Model**: Entities có business logic methods
- ✅ **Zero Duplication**: BaseRepository được reuse từ Infrastructure.EF
- ✅ **Clean Separation**: Domain không phụ thuộc EF Core
- ✅ **All Tests Pass**: Existing tests vẫn pass (sau khi update mocks)
- ✅ **API Works**: Tất cả endpoints hoạt động bình thường

---

## 🎉 Next Steps

Sau khi refactor xong:

1. **Apply to other services** (IdentityService, SearchService)
2. **Add Integration Tests** sử dụng TestContainers
3. **Implement Domain Events** với MediatR
4. **Add MongoDB support** (Infrastructure.Mongo) cho SearchService
5. **Add Specification Pattern** cho complex queries
6. **Add CQRS** nếu cần (với MediatR)

Chúc may mắn! 🚀
