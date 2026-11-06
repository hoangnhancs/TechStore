# Ghi chú: deploy ProductService lên Azure + tự động hóa bằng GitHub Actions

Tài liệu này ghi lại **toàn bộ** quy trình thực hành, theo đúng thứ tự thời gian đã làm — từ bước tay đầu tiên (tạo resource group) tới khi có pipeline CI/CD chạy tự động. Mục đích: hiểu rõ từng bước làm gì, vì sao làm, và bước nào trong pipeline tương ứng với bước tay nào.

---

# PHẦN 1 — Quyết định kiến trúc (chốt trước khi bắt tay vào)

| Quyết định | Chọn | Vì sao |
|---|---|---|
| Compute chạy container | **Azure Container Apps** (không phải App Service) | Có scale-to-zero (không chạy = không tính tiền), hợp tư duy tiết kiệm chi phí đã áp dụng với Neon trước đó. |
| RabbitMQ | **CloudAMQP free tier** | RabbitMQ hiện chỉ chạy local qua docker-compose, Azure không với tới được. Trỏ sang broker free trên cloud — code không đổi gì. |
| Database | **Giữ nguyên Neon** | Neon đã public sẵn, dùng lại connection string hiện có, không cần thêm hạ tầng. |
| Port expose ra ngoài | Chỉ **WebApi (5001)**, bỏ **gRPC (7777)** | Container Apps ingress chuẩn chỉ map 1 port. Chưa có service nào khác gọi gRPC ProductService từ ngoài Azure nên chưa cần. |
| Cách deploy lần đầu | **Azure CLI thủ công trước, GitHub Actions sau** | Hiểu rõ từng bước thật trước khi tự động hóa — pipeline lỗi thì biết ngay do script CI hay do app/config. |
| Xác thực GitHub → Azure | **OIDC** (không phải Service Principal + client secret) | Không có secret dài hạn nào lưu lại để bị lộ/cần xoay vòng — mỗi lần chạy GitHub và Azure tự trao đổi token tạm thời. |

---

# PHẦN 2 — Các bước thủ công đã chạy (ngày 1)

## 2.1. Chuẩn bị
- `az login` — đăng nhập Azure CLI bằng tài khoản cá nhân.
- Tạo tài khoản [CloudAMQP](https://www.cloudamqp.com/) free tier, lấy AMQPS connection string.
- Connection string Neon Postgres hiện có (copy từ cấu hình ProductService).

## 2.2. Tạo resource group
```bash
az group create --name techstore-rg --location southeastasia
```
"Resource group" là 1 cái thùng chứa logic — mọi tài nguyên Azure liên quan tới project này (ACR, Container Apps environment, container app...) đều nằm trong 1 group để dễ quản lý/xóa chung.

## 2.3. Đăng ký resource provider
Lần đầu tạo ACR bị lỗi:
```
(MissingSubscriptionRegistration) The subscription is not registered to use namespace 'Microsoft.ContainerRegistry'.
```
→ Subscription Azure mới chưa từng "bật" dùng dịch vụ đó lần nào. Fix:
```bash
az provider register --namespace Microsoft.ContainerRegistry --wait
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.OperationalInsights --wait
```
⚠️ **Bài học quan trọng**: `az provider register` không có `--wait` thì trả lệnh về ngay lập tức dù việc đăng ký vẫn chạy ngầm vài phút. Thiếu `--wait` đã khiến bước sau (`containerapp env create`) chạy trước khi đăng ký xong — tưởng tạo thành công nhưng thực ra fail, phát hiện muộn ở bước `env show` báo `ResourceNotFound`.

## 2.4. Azure Container Registry (ACR) — nơi lưu Docker image
```bash
az acr create --resource-group techstore-rg --name techstoreacr --sku Basic
az acr login --name techstoreacr
```
`az acr login` dùng session `az login` sẵn có (Azure AD) để xác thực — **không cần** username/password ở bước này.

## 2.5. Build + push Docker image
```bash
docker build -f server/ProductService/Dockerfile -t product-svc:v1 .
docker tag product-svc:v1 techstoreacr.azurecr.io/product-svc:v1
docker push techstoreacr.azurecr.io/product-svc:v1
```
**Build context phải là gốc repo** (dấu `.` cuối lệnh build), không phải thư mục `server/ProductService/` — vì Dockerfile `COPY` nhiều project dùng chung (`BuildingBlocks`, `Contract`), đúng y cách `docker-compose.yml` khai báo `context: .`.

## 2.6. Container Apps environment
```bash
az containerapp env create --name techstore-env --resource-group techstore-rg --location southeastasia
az containerapp env show --name techstore-env --resource-group techstore-rg --query id -o tsv
```
"Environment" là lớp hạ tầng dùng chung (mạng, Log Analytics) cho nhiều container app sau này — tạo 1 lần, nhiều container app có thể share chung 1 environment. Lệnh `show` lấy `environmentId` (dạng `/subscriptions/.../managedEnvironments/techstore-env`) để dùng ở bước deploy.

## 2.7. Credential ACR cho Container Apps pull image
```bash
az acr update -n techstoreacr --admin-enabled true
az acr credential show --name techstoreacr
```
Đây là **kiểu xác thực khác** với 2.4 — `az acr login` dùng session AAD cá nhân, còn Container Apps (lúc chạy, không phải lúc mình gõ lệnh) cần username/password "cổ điển" để tự pull image, không biết gì về session AAD của mình.

## 2.8. Soạn file cấu hình secret
- `azure/containerapp-product-svc.example.yaml` — **có track git**, chỉ chứa placeholder, làm mẫu.
- `azure/containerapp-product-svc.yaml` — **gitignored**, chứa giá trị thật.

Cấu trúc file: `secrets:` là nơi khai báo **giá trị** thật 1 lần (name + value); chỗ khác (`env[].secretRef`, `registries[].passwordSecretRef`) chỉ **trỏ tên**, không lặp lại giá trị — giống Secret của Kubernetes/GitHub Actions. Lúc điền tay từng dính 2 lỗi:
1. Lỡ điền giá trị thật vào thẳng file `.example.yaml` (file có track git) thay vì file thật — phát hiện kịp qua `git status` (chưa add/commit gì), sửa lại bằng cách tách file.
2. Nhầm giữa "tên" secret và "giá trị" secret — dán nhầm password ACR thật vào field `passwordSecretRef` (chỗ đó chỉ nhận **tên**, vd `acr-password`, giá trị thật nằm ở phần `secrets:` riêng).

## 2.9. Deploy container app — gặp 2 lỗi CLI, cuối cùng đổi hướng
Dự định dùng:
```bash
az containerapp create --name product-svc --resource-group techstore-rg --yaml azure/containerapp-product-svc.yaml
```
- Lỗi 1: `the following arguments are required: --name/-n` → CLI vẫn bắt buộc `--name` dù đã có trong YAML. Thêm `--name product-svc --environment techstore-env`.
- Lỗi 2: `400 Bad Request — The JSON value could not be converted to System.Boolean` → bug/quirk của CLI khi kết hợp `--yaml` với `--name`/`--environment` ở lệnh `create` (đã thử bỏ `location`/`type`/`name` khỏi YAML, kiểm tra BOM — không phải nguyên nhân, chưa tìm ra root cause chính xác).

**Workaround**: deploy lần đầu bằng **flag trực tiếp** thay vì `--yaml`, viết thành script `azure/deploy-product-svc.ps1` (gitignored) cho đỡ gõ tay + đỡ lộ secret ra màn hình command line dài:
```powershell
az containerapp create `
  --name product-svc --resource-group techstore-rg --environment techstore-env `
  --image techstoreacr.azurecr.io/product-svc:v1 `
  --registry-server techstoreacr.azurecr.io --registry-username techstoreacr --registry-password "..." `
  --target-port 5001 --ingress external `
  --secrets "jwt-key=..." "cloudinary-secret=..." "rabbitmq-conn=..." "db-conn=..." `
  --env-vars "ASPNETCORE_ENVIRONMENT=Docker" "ConnectionStrings__DefaultConnection=secretref:db-conn" ...
```
→ Chạy thành công: `provisioningState: Succeeded`, `runningStatus: Running`.

**Lưu ý cho tương lai**: `--yaml` vẫn dùng tốt cho `az containerapp update` (lệnh update không dính bug 400/Boolean như `create`) — nhưng **vẫn phải truyền `--name`**, CLI không tự suy ra tên app từ nội dung YAML (thiếu sẽ báo lỗi `(--name | --ids) are required`). Nên **mọi lần sửa cấu hình sau** (đổi env var, đổi secret...) dùng:
```bash
az containerapp update --name product-svc --resource-group techstore-rg --yaml azure/containerapp-product-svc.yaml
```

## 2.10. Verify
Mở URL public (`az containerapp show ... --query properties.configuration.ingress.fqdn`):
```
https://product-svc.kindisland-948abac6.southeastasia.azurecontainerapps.io/api/products/all
```
Ra JSON sản phẩm thật từ Neon → xác nhận container không chỉ "Running" ở mức process mà app .NET bên trong thật sự bind đúng port + kết nối DB đúng.

---

# PHẦN 3 — Thiết lập GitHub Actions (ngày 2)

## 3.1. Thiết lập xác thực OIDC (chạy 1 lần)
File `azure/setup-github-oidc.ps1` (gitignored) làm 4 việc:

1. **Tạo Azure AD App Registration** — 1 "danh tính" đại diện cho GitHub Actions trong Azure AD:
   ```powershell
   az ad app create --display-name "techstore-github-actions"
   ```
2. **Tạo Service Principal** gắn với app đó (để app có thể được gán quyền trên tài nguyên):
   ```powershell
   az ad sp create --id $APP_ID
   ```
3. **Gán quyền Contributor**, scope **chỉ trong** resource group `techstore-rg` (không phải cả subscription — để GitHub Actions không đụng được gì ngoài phạm vi project):
   ```powershell
   az role assignment create --assignee $APP_ID --role Contributor --scope "/subscriptions/.../resourceGroups/techstore-rg"
   ```
4. **Tạo federated credential** — "quy tắc tin tưởng": Azure AD sẽ chấp nhận token OIDC từ GitHub Actions, **chỉ khi** token đó phát hành cho đúng repo + branch này:
   ```powershell
   az ad app federated-credential create --id $APP_ID --parameters "@file.json"
   ```
   Nội dung quy tắc: `issuer: https://token.actions.githubusercontent.com`, `subject: repo:hoangnhancs/TechStore:ref:refs/heads/main`.

   ⚠️ **3 lỗi gặp phải ở đúng bước này**:
   - Truyền JSON inline trực tiếp trên PowerShell → lỗi parse (PowerShell escape quote lồng nhau rất dễ hỏng, lỗi kinh điển của Azure CLI trên Windows). Fix: ghi JSON ra file tạm, dùng `--parameters "@file"` thay vì JSON string trực tiếp.
   - Chạy lại **chỉ đúng dòng cuối** (không chạy các dòng gán biến phía trên trong cùng phiên) → biến `$APP_ID`/`$credFile` rỗng → lệnh `az` nhận tham số rỗng, báo thiếu `--id`/`--parameters`. Bài học: các dòng PowerShell có biến phải chạy **chung 1 lần, cùng 1 phiên** — biến không tự lưu lại giữa các lần chạy riêng lẻ.
   - Sau khi sửa và chạy đúng cả khối lệnh cùng lúc → thành công.

Kết quả: 3 giá trị `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — thêm vào **GitHub repo Settings → Secrets and variables → Actions**.

## 3.2. Workflow file — `.github/workflows/deploy-product-svc.yml`
```yaml
on:
  push:
    branches: [main]
    paths: ["server/ProductService/**", "server/BuildingBlocks/**", "server/Contract/**"]
  workflow_dispatch:   # cho phép bấm "Run workflow" chạy tay từ tab Actions

permissions:
  id-token: write      # bắt buộc cho OIDC
  contents: read
```
Chỉ tự trigger khi đổi code trong 3 thư mục đó (tránh build/deploy lãng phí khi sửa service khác). `workflow_dispatch` cho phép test ngay không cần đổi code.

## 3.3. Lỗi gặp khi chạy pipeline lần đầu
```
AADSTS70025: The client has no configured federated identity credentials.
```
→ Vì ở 3.1, bước tạo federated credential thực ra **chưa từng chạy thành công** (2 lỗi liệt kê ở trên), dù các bước 1-3 (app/sp/role) đã xong. Debug bằng:
```bash
az ad app federated-credential list --id <APP_ID>    # ra rỗng → xác nhận chưa có
az ad app list --display-name "techstore-github-actions"   # kiểm tra có bị tạo app trùng không (không bị)
```
Sau khi tạo lại federated credential đúng cách (3.1) → chạy lại pipeline → **thành công toàn bộ**.

## 3.4. Dọn dẹp: Railway check gây nhiễu
Phát hiện mỗi lần push, GitHub hiện thêm 1 loạt check "Railway" (tích hợp từ trước, không liên quan Azure) — do Railway có GitHub App riêng tự trigger deploy check cho mọi service nó theo dõi. Vì Railway đã ngưng chạy, xử lý: vào `github.com/settings/installations` → Configure Railway app → gỡ quyền truy cập repo này (hoặc Uninstall hẳn) — chỉ cắt kết nối GitHub↔Railway, không ảnh hưởng service/data đã có trên Railway.

---

# PHẦN 4 — Mỗi step trong pipeline tương ứng bước tay nào

| # | Step trong `deploy-product-svc.yml` | Tương ứng bước tay | Khác biệt |
|---|---|---|---|
| 1 | **Checkout code** (`actions/checkout@v4`) | — (không có bước tay tương đương) | Máy mình luôn sẵn code; runner GitHub thì trống, phải tự clone repo về trước khi làm gì tiếp. |
| 2 | **Đăng nhập Azure (OIDC)** (`azure/login@v2`) | 2.1 `az login` | Máy mình login bằng tài khoản cá nhân (trình duyệt); CI login bằng OIDC (App Registration + federated credential ở Phần 3.1) — không có tài khoản người, không có secret dài hạn. |
| 3 | **Đăng nhập ACR** (`az acr login`) | 2.4 `az acr login` | Giống hệt, chỉ chạy tự động. Dùng chung quyền Azure vừa login ở step 2 (Service Principal có quyền Contributor trên resource group, bao gồm cả ACR). |
| 4 | **Build + push Docker image** | 2.5 (`docker build/tag/push`) | Giống hệt lệnh, chỉ khác: tag theo **git sha** (`github.sha`) thay vì cố định `v1` — mỗi lần deploy ra 1 tag riêng, truy được đúng commit nào đang chạy production. |
| 5 | **Cập nhật Container App sang image mới** (`az containerapp update --image ...`) | 2.9 (`containerapp create`) | Khác bản chất: bước tay là **tạo mới** container app (kèm toàn bộ secret/env/ingress); pipeline chỉ **update image** cho container app đã tồn tại sẵn — không đụng tới secret/env vars (những cái đó set 1 lần lúc tạo, giữ nguyên qua mọi lần update sau). |

## Những bước tay **không** nằm trong pipeline (và lý do đúng là không nên có)
Những bước ở Phần 2.2 → 2.8 và toàn bộ Phần 3.1 là **hạ tầng một-lần** (provision infrastructure) — tạo resource group, ACR, environment, container app lần đầu, setup OIDC. Pipeline CI/CD chỉ lo phần **lặp lại mỗi lần có code mới** (build → push → update image), không nên tự động hóa luôn phần tạo hạ tầng (rủi ro cao hơn, ít khi cần chạy lại, và cần review kỹ hơn do đụng tới billing/quyền hạn).

---

# PHẦN 5 — Giới hạn hiện tại (biết để không nhầm là "xong hết")

- Chỉ mới deploy **1/9 service** (ProductService) — các service khác chưa động tới.
- gRPC (7777) chưa expose ra ngoài Container Apps.
- `Jwt__Issuer`/`Jwt__Audience` vẫn trỏ `localhost` — endpoint cần auth thật sẽ không hoạt động đúng cho tới khi trỏ URL thật.
- Pipeline chỉ cập nhật **image**, không tự động xoay/cập nhật secret — đổi secret vẫn phải làm tay qua `containerapp update --yaml`.
