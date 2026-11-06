# Ghi chú: thêm Azure Blob Storage làm nơi lưu ảnh (thay thế/song song Cloudinary)

Mục tiêu: ProductService đang lưu ảnh qua Cloudinary — thử thêm Azure Blob Storage làm lựa chọn thứ 2, chuyển qua lại bằng config, không sửa logic nghiệp vụ.

---

## PHẦN 1 — Code (làm 1 lần, không cần lặp lại mỗi lần tạo storage mới)

### 1.1. Kiến trúc có sẵn — vì sao chỉ cần thêm 1 class
`CreateNewProductHandler`/`UpdateProductHandler` chỉ phụ thuộc interface `IPhotoService` ([IPhotoService.cs](../server/PhotoService/IPhotoService.cs)), không biết gì về Cloudinary cụ thể — đúng kiểu Strategy pattern sẵn có trong code. Nên thêm nhà cung cấp lưu trữ mới chỉ cần: 1 class implement `IPhotoService`, không đụng tới handler nào.

### 1.2. Thêm package
```bash
dotnet add server/PhotoService/PhotoService.csproj package Azure.Storage.Blobs
```
⚠️ **Lỗi gặp**: `NU1605: Detected package downgrade: Microsoft.Extensions.Options from 10.0.9 to 9.0.11` — `Azure.Storage.Blobs` kéo theo dependency yêu cầu `Microsoft.Extensions.Options >= 10.0.9`, trong khi `PhotoService.csproj` đang pin `9.0.11`. Fix: bump `Microsoft.Extensions.Options` và `Microsoft.Extensions.Options.ConfigurationExtensions` lên `10.0.9` trong `.csproj`.

### 1.3. File mới
- [`Configs/AzureBlobSettings.cs`](../server/PhotoService/Configs/AzureBlobSettings.cs) — `ConnectionString`, `ContainerName`.
- [`Services/AzureBlobPhotoService.cs`](../server/PhotoService/Services/AzureBlobPhotoService.cs) — implement thẳng `IPhotoService` bằng `BlobContainerClient` (không cần tầng trung gian như `ICloudinaryServices`, vì Blob SDK đã đủ gọn):
  - `UploadAsync`: tạo blob name `{folder}/{Guid.NewGuid()}{extension}`, upload, trả `PublicId` = blob name, `Url` = `blobClient.Uri.AbsoluteUri`.
  - `DeleteAsync`: `DeleteIfExistsAsync()`.
  - `GetUrl`: build URL trực tiếp từ blob name (khác Cloudinary — bản Cloudinary hiện tại method này đang `throw new NotImplementedException()`).

### 1.4. Switch qua config — [`Extensions/ServiceCollectionExtensions.cs`](../server/PhotoService/Extensions/ServiceCollectionExtensions.cs)
```csharp
var provider = configuration["PhotoStorage:Provider"] ?? "Cloudinary";

if (string.Equals(provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
{
    services.Configure<AzureBlobSettings>(configuration.GetSection("AzureBlobSettings") ?? throw ...);
    services.AddScoped<IPhotoService, AzureBlobPhotoService>();
}
else
{
    services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings") ?? throw ...);
    services.AddScoped<ICloudinaryServices, CloudinaryServices>();
    services.AddScoped<IPhotoService, PhotoService>();
}
```
Không set gì → mặc định `Cloudinary`, hành vi y như cũ. Quyết định này chạy **1 lần lúc app start** (build DI container), không phải runtime-switch theo từng request — khác với `PaymentServiceFactory` (chọn `IPaymentService` theo từng đơn hàng qua `IEnumerable<IPaymentService>` + factory, vì phương thức thanh toán thực sự thay đổi theo từng request). Áp dụng pattern `IEnumerable` + factory như Payment vào đây sẽ là over-engineering vì không có nhu cầu chọn provider theo từng lần upload ảnh.

### 1.5. Config keys cần có (thêm vào `.env.example`)
```
PhotoStorage__Provider=
AzureBlobSettings__ConnectionString=
AzureBlobSettings__ContainerName=
```

---

## PHẦN 2 — Hạ tầng: tạo Storage Account + Container (tự chạy từng lệnh)

### 2.1. Đăng ký provider
```bash
az provider register --namespace Microsoft.Storage --wait
```

### 2.2. Tạo Storage Account
```bash
az storage account create --name techstorephotosdemo --resource-group testRG --location southeastasia --sku Standard_LRS
```

### 2.3. Tạo Container — ⚠️ lỗi gặp phải, mất nhiều bước mới ra
Lệnh dự định (muốn container public để URL ảnh truy cập trực tiếp được, giống Cloudinary):
```bash
az storage container create --name product-photos --account-name techstorephotosdemo --public-access blob
```
→ Kết quả `{"created": false}` — **không có lỗi rõ ràng**, chỉ 1 field boolean, rất khó đoán nguyên nhân. Debug qua nhiều bước:

1. Nghi ngờ #1: thiếu credential → thử `--auth-mode login` (dùng AAD cá nhân) → vẫn `false`.
2. Nghi ngờ #2: quyền RBAC data-plane thiếu (có quyền *quản lý* storage account — control-plane — không tự động có quyền *đọc/ghi data* bên trong — data-plane, 2 loại quyền tách biệt trên Azure) → thử account key (key có full quyền data, không qua RBAC) → vẫn `false`.
3. Verify bằng `az storage container list -o json` → ra `[]` thật — xác nhận container **chưa từng được tạo**, không phải do đã tồn tại từ trước.
4. Kiểm tra `az storage account show --query allowBlobPublicAccess` → ra `false` — **đây mới là nguyên nhân thật**: account-level đang chặn hẳn việc set public access cho bất kỳ container nào, khiến cờ `--public-access blob` làm cả lệnh create thất bại âm thầm (CLI không in lỗi rõ, chỉ trả `created: false`).
5. Thử tạo lại **không có** `--public-access` → `{"created": true}` — xác nhận đúng chẩn đoán.

**Fix đúng thứ tự — bật public access ở cả 2 cấp (account trước, rồi container)**:
```bash
# Cấp account — cho phép tồn tại public container
az storage account update --resource-group testRG --name techstorephotosdemo --allow-blob-public-access true

# Cấp container — set container này cụ thể là public (anonymous read blob)
az storage container set-permission --name product-photos --account-name techstorephotosdemo --account-key "<key>" --public-access blob
```
Lấy account key trước đó bằng:
```bash
az storage account keys list --resource-group testRG --account-name techstorephotosdemo --query "[0].value" -o tsv
```

**Bài học**: `created: false` của `az storage container create` **không đồng nghĩa "đã tồn tại"** — phải tự verify bằng `list -o json` để phân biệt 2 trường hợp (đã tồn tại vs tạo thất bại âm thầm). Và việc set `--public-access` trên container chỉ thành công nếu **account-level `allowBlobPublicAccess` đã bật trước** — 2 cấp độc lập, thiếu 1 trong 2 là fail.

### 2.4. Lấy connection string
```bash
az storage account show-connection-string --name techstorephotosdemo --resource-group testRG --query connectionString -o tsv
```

### 2.5. Set vào `.env` của ProductService
```
PhotoStorage__Provider=AzureBlob
AzureBlobSettings__ConnectionString=<connection string ở bước 2.4>
AzureBlobSettings__ContainerName=product-photos
```

### 2.6. Chạy thử
```bash
dotnet run --project server/ProductService
```
Test tạo/sửa sản phẩm kèm upload ảnh qua API — response trả về `Url` dạng `https://techstorephotosdemo.blob.core.windows.net/product-photos/...`, mở thẳng trên browser xem được (vì container đã public).

---

## PHẦN 3 — Xem ảnh đã upload

**1. Trực tiếp từ response API** — mở thẳng field `Url` trả về trên browser.

**2. CLI — liệt kê toàn bộ blob**:
```bash
az storage blob list --container-name product-photos --account-name techstorephotosdemo --account-key "<key>" --query "[].name" -o tsv
```

**3. Portal** — vào storage account → **Data storage → Containers** → click `product-photos` → thấy list file, có preview ảnh trực tiếp. (Khác với Postgres Flexible Server, Blob Storage **có** UI xem nội dung ngay trên portal.)

---

## PHẦN 4 — Dọn dẹp (nếu không giữ lâu dài)
```bash
az storage account delete --resource-group testRG --name techstorephotosdemo --yes
```
Nhớ trả `.env` về lại Cloudinary (xoá/comment 3 dòng `PhotoStorage__Provider`/`AzureBlobSettings__*`, hoặc đơn giản để trống `PhotoStorage__Provider` vì code đã mặc định `Cloudinary` khi để trống) sau khi xoá storage account — nếu không, lần chạy `dotnet run` tiếp theo sẽ cố connect tới storage account đã xoá.
