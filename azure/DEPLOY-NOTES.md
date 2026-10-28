# Ghi chú: deploy ProductService lên Azure (lần thực hành đầu tiên)

Mục đích file này: tổng hợp lại toàn bộ flow đã làm tay qua Azure CLI, để hiểu rõ từng bước trước khi chuyển thành GitHub Actions pipeline.

## 1. Quyết định kiến trúc đã chốt

| Quyết định | Chọn | Vì sao |
|---|---|---|
| Compute | **Azure Container Apps** (không phải App Service) | Có scale-to-zero (không chạy = không tính tiền), hợp tư duy tiết kiệm chi phí đã áp dụng với Neon. Có lệnh `az containerapp up`/`create` tiện cho thực hành. |
| RabbitMQ | **CloudAMQP free tier** | RabbitMQ hiện chỉ chạy local qua docker-compose, không public được từ Azure. Thay vì sửa code, trỏ sang 1 broker free trên cloud — code không đổi gì cả. |
| Database | **Giữ nguyên Neon** (không tạo Azure Database for PostgreSQL) | Neon đã public sẵn, dùng lại connection string hiện có, không cần thêm hạ tầng mới cho lần thực hành đầu. |
| Port expose | Chỉ **WebApi (5001)**, bỏ qua gRPC (7777) | Container Apps ingress chuẩn chỉ map 1 port ra ngoài. Chưa có service nào khác gọi gRPC của ProductService từ ngoài Azure nên không cần thiết cho lần đầu. |
| Cách deploy | **Azure CLI thủ công trước, GitHub Actions sau** | Hiểu rõ từng bước thật trước khi tự động hóa — pipeline lỗi sẽ biết ngay là do script CI hay do bản thân app/config. |

## 2. Chuẩn bị trước khi bắt đầu

- `az login` — đăng nhập Azure CLI.
- Tạo tài khoản [CloudAMQP](https://www.cloudamqp.com/) free tier ("Little Lemur"), lấy AMQPS connection string.
- Connection string Neon Postgres hiện có (copy từ `.env`/`appsettings` của ProductService).
- Image build từ `server/ProductService/Dockerfile` — **context phải là gốc repo**, không phải thư mục `server/ProductService/`, vì Dockerfile `COPY` nhiều project dùng chung (`BuildingBlocks`, `Contract`...), giống hệt cách `docker-compose.yml` khai báo `context: .`.

## 3. Các bước đã chạy (theo đúng thứ tự)

### 3.1. Resource group
```bash
az group create --name techstore-rg --location southeastasia
```

### 3.2. Đăng ký resource provider (bắt buộc với subscription mới, chưa từng dùng dịch vụ đó)
```bash
az provider register --namespace Microsoft.ContainerRegistry --wait
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.OperationalInsights --wait
```
⚠️ **Luôn thêm `--wait`.** Không có flag này, lệnh trả về ngay dù việc đăng ký vẫn chạy ngầm vài phút — dễ bị lừa tưởng đã xong rồi chạy bước sau, dẫn tới resource "được tạo" nhưng thực ra fail âm thầm.

### 3.3. Azure Container Registry (ACR)
```bash
az acr create --resource-group techstore-rg --name techstoreacr --sku Basic
az acr login --name techstoreacr
```
`az acr login` dùng session `az login` sẵn có để xác thực — không cần nhập password thủ công ở bước này.

### 3.4. Build + push image
```bash
docker build -f server/ProductService/Dockerfile -t product-svc:v1 .
docker tag product-svc:v1 techstoreacr.azurecr.io/product-svc:v1
docker push techstoreacr.azurecr.io/product-svc:v1
```

### 3.5. Container Apps environment
```bash
az containerapp env create --name techstore-env --resource-group techstore-rg --location southeastasia
az containerapp env show --name techstore-env --resource-group techstore-rg --query id -o tsv
```
Lệnh `show` trả về `environmentId` — dùng ở bước deploy.

### 3.6. Lấy credential ACR (cho Container Apps pull image)
```bash
az acr update -n techstoreacr --admin-enabled true
az acr credential show --name techstoreacr
```
Lưu ý: `az acr login` (3.3) và credential admin (3.6) là **2 kiểu xác thực khác nhau** — login dùng session Azure AD, còn Container Apps pull image cần username/password "cổ điển" lấy từ `credential show`.

### 3.7. Deploy container app
Dự định dùng `az containerapp create --yaml <file>` (file YAML khai báo toàn bộ secret/env thay vì gõ tay command line dài) nhưng **gặp bug CLI**: kết hợp `--yaml` với `--name`/`--environment` bị lỗi `400 Bad Request — The JSON value could not be converted to System.Boolean` (chưa rõ nguyên nhân gốc, đã thử bỏ `location`/`type`/`name` khỏi YAML, kiểm tra BOM — không phải do những cái đó).

**Workaround đã dùng**: deploy lần đầu bằng flag trực tiếp (`az containerapp create --secrets ... --env-vars ...`), viết thành script `.ps1` cho đỡ gõ tay + đỡ lộ secret ra lịch sử command line thấy được ngay trên màn hình. Deploy thành công, `provisioningState: Succeeded`.

**Quan trọng**: `--yaml` vẫn dùng tốt cho `az containerapp **update**` (không bug như `create`) — nên **từ lần sau**, sửa cấu hình (đổi image version, thêm biến môi trường...) thì sửa file `azure/containerapp-product-svc.yaml` rồi chạy:
```bash
az containerapp update --resource-group techstore-rg --yaml azure/containerapp-product-svc.yaml
```

### 3.8. Verify
```
https://product-svc.<random>.southeastasia.azurecontainerapps.io/api/products/all
```
Mở URL, thấy JSON sản phẩm thật từ Neon → xác nhận: container chạy đúng, kết nối DB đúng, không chỉ "Running" ở mức process.

Xem log khi cần debug:
```bash
az containerapp logs show --name product-svc --resource-group techstore-rg --follow
```

## 4. Cách quản lý secret

- `azure/containerapp-product-svc.example.yaml` — **track trong git**, chỉ chứa placeholder `<...>`, dùng làm mẫu.
- `azure/containerapp-product-svc.yaml` — **gitignored**, chứa giá trị thật, dùng cho `containerapp update` các lần sau.
- `azure/deploy-product-svc.ps1` — **gitignored**, script dùng 1 lần để tạo mới (workaround bug `create --yaml`).
- Trong file YAML: `secrets:` là nơi khai báo giá trị thật 1 lần (name + value), các chỗ khác (`env[].secretRef`, `registries[].passwordSecretRef`) chỉ **trỏ tên**, không lặp lại giá trị — giống cơ chế Secret của Kubernetes/GitHub Actions.

## 5. Lỗi/bài học đã gặp (đáng nhớ để không lặp lại)

1. Dán `<giá trị>` (placeholder còn nguyên dấu `<`) thẳng vào bash → bị hiểu thành input-redirect từ file, không phải lỗi cú pháp lệnh.
2. Nhầm giữa **tên** secret (`acr-password`) và **giá trị** secret — có lúc dán nhầm password thật vào field `passwordSecretRef` (chỗ đó chỉ nhận tên).
3. `az provider register` không `--wait` → chạy bước sau quá sớm → tưởng resource đã tạo nhưng thực ra chưa (`ResourceNotFound` khi query lại).
4. `az containerapp create --yaml` + `--name`/`--environment` → lỗi 400 Boolean conversion, chưa rõ root cause — tránh dùng combo này, deploy lần đầu bằng flag trực tiếp, dùng `--yaml` cho `update`.
5. Soát kỹ trước khi điền secret: file `.example.yaml` (có track git) với file thật (gitignored) phải tách biệt ngay từ đầu, tránh lỡ tay điền secret thật vào file sẽ bị commit.

## 6. Giới hạn hiện tại (biết để không nhầm là "xong hết")

- Chỉ mới deploy **1/9 service** (ProductService) — các service khác (OrderService, PaymentService...) chưa động tới.
- gRPC (7777) chưa expose ra ngoài — nếu sau này cần service khác trên Azure gọi gRPC của ProductService thì phải cấu hình thêm (Container Apps hỗ trợ multi-port qua `additionalPortMappings`, chưa làm).
- `Jwt__Issuer`/`Jwt__Audience` vẫn trỏ `localhost` — endpoint cần auth thật sẽ không hoạt động đúng cho tới khi trỏ URL thật.
- Chưa có CI/CD — build/push/deploy vẫn đang làm tay.

## 7. Bước tiếp theo

Viết GitHub Actions workflow tự động hóa đúng chuỗi bước ở mục 3 (build → push ACR → `containerapp update --yaml`) mỗi khi push code — việc này sẽ làm sau khi note này được xác nhận đã hiểu đúng flow.
