# Ghi chú: deploy ProductService lên Azure App Service (thực hành, không dùng lâu dài)

Tài liệu này viết theo đúng khung của `azure/DEPLOY-NOTES.md` (bản Container Apps), nhưng cho **Azure App Service** — chỉ để thực hành/so sánh. Sau khi thực hành xong, project vẫn tiếp tục dùng **Azure Container Apps** làm hạ tầng chính (xem Phần 5 để biết vì sao).

Để không đụng/ảnh hưởng gì tới hạ tầng ACA đang có, dùng **resource group riêng** cho việc thực hành này: `techstore-appservice-rg`. Xong việc có thể xóa sạch bằng 1 lệnh (`az group delete --name techstore-appservice-rg --yes`) mà không ảnh hưởng `techstore-rg`.

---

# PHẦN 1 — Quyết định kiến trúc (so với bản ACA)

| Quyết định | Chọn (bản thực hành này) | Vì sao |
|---|---|---|
| Compute chạy container | **Azure App Service (Linux, Web App for Containers)** | Mục đích thực hành — so sánh với Container Apps đã làm. |
| Resource group | **`techstore-appservice-rg`** (tách riêng, không dùng `techstore-rg`) | Tránh đụng/làm lệch cấu hình ACA đang chạy thật; dễ dọn sạch sau khi thực hành xong. |
| ACR | **Tạo mới riêng hoặc dùng lại `techstoreacr`** | Dùng lại ACR cũ cũng được (ACR không bị ảnh hưởng gì bởi việc có thêm 1 App Service pull image từ nó) — ghi chú dưới đây giả định dùng lại `techstoreacr`. |
| Port expose | Chỉ **WebApi (5001)**, bỏ gRPC (7777) | Giống lý do ở bản ACA, nhưng ở App Service đây là **bắt buộc** chứ không phải lựa chọn — xem Phần 5. |
| RabbitMQ / Database | **Giữ nguyên CloudAMQP + Neon** | Không liên quan tới compute layer, dùng lại y nguyên cấu hình đã có. |
| Cách deploy | **CLI thủ công** (như ACA đã làm ở ngày 1) | Để thấy rõ sự khác biệt từng bước so với ACA. |

---

# PHẦN 2 — Các bước thủ công

## 2.1. Chuẩn bị
- `az login` — giống hệt ACA, dùng lại session đã đăng nhập.
- Image đã có sẵn trên `techstoreacr.azurecr.io/product-svc:v1` (nếu đã làm bản ACA) — không cần build/push lại trừ khi muốn tag riêng cho bản thực hành này.

## 2.2. Tạo resource group riêng
```bash
az group create --name techstore-appservice-rg --location southeastasia
```
Tách riêng khỏi `techstore-rg` — lý do nêu ở Phần 1.

## 2.3. Đăng ký resource provider
App Service dùng namespace khác với Container Apps:
```bash
az provider register --namespace Microsoft.Web --wait
```
(`Microsoft.ContainerRegistry` thường đã registered từ lúc làm ACA rồi, không cần lại — kiểm tra bằng `az provider show --namespace Microsoft.ContainerRegistry --query registrationState`.)

## 2.4. ACR — dùng lại, không cần tạo mới
Nếu đã có `techstoreacr` từ bản ACA, **dùng lại luôn**, không cần tạo ACR riêng cho App Service — 1 ACR có thể làm nguồn image cho nhiều compute platform khác nhau cùng lúc.
```bash
az acr login --name techstoreacr
```

## 2.5. Build + push image (nếu chưa có, hoặc muốn tag riêng)
```bash
docker build -f server/ProductService/Dockerfile -t product-svc:v1 .
docker tag product-svc:v1 techstoreacr.azurecr.io/product-svc:v1
docker push techstoreacr.azurecr.io/product-svc:v1
```
Giống hệt bước 2.5 bản ACA — build context vẫn phải là gốc repo (`.`), lý do y như cũ (Dockerfile COPY cả `BuildingBlocks`/`Contract`).

## 2.6. App Service Plan — tương đương "Container Apps environment"
```bash
az appservice plan create --name techstore-appservice-plan --resource-group techstore-appservice-rg --location southeastasia --is-linux --sku B1
```
"App Service Plan" là lớp hạ tầng compute dùng chung cho nhiều Web App sau này — giống vai trò của "Container Apps environment" ở bản ACA. **Khác biệt quan trọng**: plan này là 1 VM chạy liên tục theo SKU đã chọn (ví dụ B1), **tính tiền 24/7 dù không có request nào** — không có khái niệm scale-to-zero như Container Apps consumption plan.

## 2.7. Credential ACR để App Service tự pull image
```bash
az acr update -n techstoreacr --admin-enabled true
az acr credential show --name techstoreacr
```
Giống hệt lý do ở bản ACA (2.7): App Service lúc runtime pull image cũng không biết gì về session AAD cá nhân, cần username/password "cổ điển" riêng.

## 2.8. Tạo Web App + gán image
Khác với ACA (không có file YAML tách bạch `secrets:`/`env:`), App Service tạo App rồi set container + appsettings bằng lệnh riêng:
```bash
az webapp create \
  --name product-svc-appservice-demo \
  --resource-group techstore-appservice-rg \
  --plan techstore-appservice-plan \
  --container-image-name techstoreacr.azurecr.io/product-svc:v1
```
⚠️ Tên `product-svc-appservice-demo` phải **unique toàn cầu** trong `*.azurewebsites.net` (giống ACR phải unique trong `*.azurecr.io`) — nếu trùng tên người khác đã đặt, lệnh sẽ báo lỗi, phải đổi tên khác.

⚠️ Flag `--deployment-container-image-name` (bản cũ của lệnh `create`) và `--docker-registry-server-url/-user/-password` (bản cũ của lệnh `config container set` dưới đây) đã **deprecated** ở CLI mới — đổi thành `--container-image-name` và `--container-registry-url/-user/-password`:

```bash
az webapp config container set \
  --name product-svc-appservice-demo \
  --resource-group techstore-appservice-rg \
  --container-registry-url https://techstoreacr.azurecr.io \
  --container-registry-user techstoreacr \
  --container-registry-password "<password lấy ở 2.7>"
```

## 2.9. Set biến môi trường (appsettings) — tương đương `secrets:`/`env:` trong YAML ACA
```bash
az webapp config appsettings set \
  --name product-svc-appservice-demo \
  --resource-group techstore-appservice-rg \
  --settings \
    WEBSITES_PORT=5001 \
    ASPNETCORE_ENVIRONMENT=Docker \
    Kestrel__Endpoints__WebApi__Protocols=Http1 \
    Kestrel__Endpoints__WebApi__Url=http://+:5001 \
    ConnectionStrings__DefaultConnection="<connection string Neon>" \
    RabbitMq__ConnectionString="<CloudAMQP amqps url>" \
    Jwt__Key="<jwt key>" \
    Jwt__Issuer="https://localhost:6001" \
    Jwt__Audience="https://localhost:3000" \
    CloudinarySettings__CloudName="dukhvtyr7" \
    CloudinarySettings__ApiKey="297889177929435" \
    CloudinarySettings__ApiSecret="<cloudinary secret>"
```
**Lưu ý khác biệt với ACA**: `az webapp config appsettings set` không có khái niệm `secretRef` tách giá trị thật ra riêng — mọi giá trị (kể cả secret) đều nằm chung trong appsettings dạng key=value. Muốn tách secret ra ngoài giống kiểu `secrets:` của ACA thì phải dùng **Key Vault reference** (`@Microsoft.KeyVault(SecretUri=...)`) — phức tạp hơn, không làm trong bản thực hành này.

`WEBSITES_PORT=5001` là setting đặc biệt của App Service — báo cho platform biết container lắng nghe port nào để nó proxy request vào đúng port đó (ACA dùng `--target-port` ở ngay lệnh `create`, App Service dùng appsetting này).

⚠️ **Bug thực tế đã gặp**: chỉ set `WEBSITES_PORT=5001` là **chưa đủ** — container vẫn bind vào port 8080 (default của base image .NET 8+, biến `ASPNETCORE_HTTP_PORTS`/`ASPNETCORE_URLS` ngầm trong image), gây lỗi `Container did not respond to startup probe on port 5001 ... Port mismatch detected: the container is listening on port 8080 but the platform is probing port 5001` → app bị 503, bị restart loop. Nguyên nhân: Program.cs của ProductService chỉ đọc port qua `Kestrel:Endpoints:WebApi:Url` trong config (giống cách ACA từng set ở `deploy-product-svc.ps1`), **không hề đọc `WEBSITES_PORT`** — `WEBSITES_PORT` chỉ là tín hiệu cho App Service biết probe port nào, không tự inject vào app. Phải set thêm đúng 2 biến `Kestrel__Endpoints__WebApi__Protocols`/`Url` như trên thì Kestrel mới thực sự bind đúng 5001.

**Quy trình thực tế đã dùng để tìm ra lỗi này** (503 sau khi deploy xong, không đoán bừa mà thu hẹp dần từng tầng):

1. **Kiểm tra platform có coi app là "chạy" không** — tách biệt "lệnh deploy fail" khỏi "app chạy nhưng không trả lời được request":
   ```bash
   az webapp show --name product-svc-appservice-demo --resource-group techstore-appservice-rg --query "{state:state, availabilityState:availabilityState}"
   ```
   Kết quả `state: Running`, `availabilityState: Normal` → loại trừ khả năng lệnh `create`/`config` bị lỗi; vấn đề nằm ở tầng container/app bên trong.

2. **Gọi thẳng endpoint thật** (không qua log, qua chính request) để xác nhận app có đáp ứng không:
   ```bash
   curl -s -o /dev/null -w "%{http_code}" https://product-svc-appservice-demo.azurewebsites.net/api/products/all
   ```
   → Timeout / 503, nghĩa là platform nói "Running" nhưng container thật sự không trả lời được HTTP request nào — mâu thuẫn này chỉ ra vấn đề đang ở bên trong container, không phải ở hạ tầng App Service.

3. **Kiểm tra lại config đã set đúng chưa** trước khi nghi ngờ code — loại trừ lỗi gõ sai tay:
   ```bash
   az webapp config appsettings list ... --query "[?name=='WEBSITES_PORT']"
   az webapp config container show ...
   ```
   → `WEBSITES_PORT=5001` và image đúng, không phải lỗi config cơ bản → phải đọc log thật mới biết chuyện gì xảy ra lúc container start.

4. **Thử `az webapp log tail` trước** (cách nhanh nhất) — nhưng bị `504 Gateway Timeout` ở `/logstream`, và sau đó chạy ngầm 45s không ra gì cả. Đây là tín hiệu SCM site (Kudu) chưa sẵn sàng hoặc stream không ổn định, **không phải** là bằng chứng "không có lỗi" — nên không dừng ở đây mà đổi cách khác.

5. **Đổi sang `az webapp log download`** — lấy snapshot file log thật thay vì stream live (đáng tin hơn vì không phụ thuộc kết nối real-time):
   ```bash
   az webapp log config --name product-svc-appservice-demo --resource-group techstore-appservice-rg --docker-container-logging filesystem
   az webapp log download --name product-svc-appservice-demo --resource-group techstore-appservice-rg --log-file logs.zip
   ```
   Giải nén ra thấy ngay có file `StartupLogs/*_failure*` — tín hiệu mạnh là container start bị fail thật, không phải do chậm.

6. **Grep log tìm từ khóa lỗi** (`error|fail|exited|timeout`) trong `LogFiles/*_docker.log` thay vì đọc tuần tự từ đầu (file dài, phần lỗi thường nằm giữa) → tìm ra đúng dòng `Failed to start site`.

7. **Đọc full `LastErrorDetails` của dòng đó** — Azure ghi rất rõ, không cần đoán:
   > `Container did not respond to startup probe on port 5001 ... Port mismatch detected: the container is listening on port 8080 but the platform is probing port 5001.`

   → Nguyên nhân lộ ra trực tiếp từ text log, không phải suy luận: container bind nhầm port.

8. **Đối chiếu lại với bản ACA đã từng làm thành công** — nhớ ra `deploy-product-svc.ps1` có set `Kestrel__Endpoints__WebApi__Url=http://+:5001` tường minh, còn file App Service mới viết thì thiếu đúng 2 biến này → xác định chính xác thứ còn thiếu, không phải đoán ngẫu nhiên.

**Bài học quy trình**: khi gặp lỗi mơ hồ ("503", "không chạy được") trên 1 service đã deploy, đi theo thứ tự **platform state → gọi thẳng endpoint → config đã set đúng chưa → log snapshot (không phải live tail) → grep từ khóa lỗi → đọc nguyên văn error message → đối chiếu với lần deploy tương tự đã chạy được**. Mỗi bước chỉ để loại trừ 1 tầng khả năng, không nhảy thẳng vào sửa khi chưa thấy bằng chứng cụ thể.

## 2.10. Verify
```bash
az webapp show --name product-svc-appservice-demo --resource-group techstore-appservice-rg --query defaultHostName -o tsv
```
Mở `https://<defaultHostName>/api/products/all` — ra JSON sản phẩm từ Neon thì xác nhận container chạy đúng, bind đúng port.

⚠️ App Service container cold-start **chậm hơn** Container Apps đáng kể ở lần đầu/ sau khi đổi image — bình thường phải đợi 1-2 phút, xem log bằng:
```bash
az webapp log tail --name product-svc-appservice-demo --resource-group techstore-appservice-rg
```

---

# PHẦN 3 — CI/CD GitHub Actions (nếu muốn tự động hóa, tương đương Phần 3 bản ACA)

Phần OIDC setup (App Registration, Service Principal, role assignment, federated credential) **giữ nguyên 100%** như bản ACA — không cần làm lại, chỉ cần thêm role assignment Contributor cho scope resource group mới:
```bash
az role assignment create --assignee <APP_ID> --role Contributor --scope "/subscriptions/<sub-id>/resourceGroups/techstore-appservice-rg"
```

Workflow chỉ khác ở bước cuối — thay vì `az containerapp update --image`, dùng:
```yaml
- name: Cập nhật Web App sang image mới
  run: |
    az webapp config container set \
      --name product-svc-appservice-demo \
      --resource-group techstore-appservice-rg \
      --docker-custom-image-name "${{ env.IMAGE_TAG }}"
```
Các step còn lại (checkout, `azure/login` OIDC, `az acr login`, build+push) **giữ nguyên y hệt** workflow ACA — chỉ đổi đúng 1 step cuối.

---

# PHẦN 4 — Mỗi bước App Service tương ứng bước nào ở ACA

| # | Bước App Service | Tương ứng bước ACA | Khác biệt chính |
|---|---|---|---|
| 2.2 | `az group create` (RG riêng) | 2.2 | Dùng RG riêng để tách khỏi hạ tầng ACA đang chạy thật |
| 2.3 | Register `Microsoft.Web` | 2.3 (`Microsoft.App`) | Namespace khác, cùng cơ chế |
| 2.4–2.5 | ACR login + build/push | 2.4–2.5 | Giống hệt, dùng lại ACR cũ |
| 2.6 | `az appservice plan create` | 2.6 (`containerapp env create`) | Cùng vai trò "lớp hạ tầng dùng chung", nhưng Plan là VM luôn chạy, Environment hỗ trợ scale-to-zero |
| 2.7 | `az acr update --admin-enabled` | 2.7 | Giống hệt |
| 2.8 | `az webapp create` + `config container set` | 2.9 (`containerapp create`) | App Service tách làm 2 lệnh (tạo app, rồi set image); ACA làm 1 lệnh |
| 2.9 | `az webapp config appsettings set` | 2.8 (file YAML `secrets:`/`env:`) | App Service không tách riêng "giá trị thật" như ACA — mọi thứ chung 1 chỗ trừ khi dùng Key Vault reference |
| 2.10 | Verify qua `*.azurewebsites.net` | 2.10 (`*.azurecontainerapps.io`) | Giống hệt về mục đích |

---

# PHẦN 5 — Vì sao vẫn chọn ACA cho project thật (không chỉ là thực hành)

1. **Port đơn** — App Service Linux container chỉ expose **1 port** (`WEBSITES_PORT`). ProductService có 2 port (REST 5001 + gRPC 7777 trên Kestrel) — ACA xử lý multi-port qua `additionalPortMappings`, App Service **không hỗ trợ** việc này native. Muốn giữ cả gRPC thì phải tách container/side-car, phức tạp hơn nhiều.
2. **Không scale-to-zero** — App Service Plan luôn chạy, tính tiền 24/7 bất kể traffic. ACA consumption plan (đang dùng, min replicas = 0) phù hợp hơn với tư duy tối ưu chi phí đã áp dụng cho Neon.
3. **Secret management kém tách bạch hơn** — không có cơ chế tương đương `secrets:`/`secretRef` của ACA trừ khi tích hợp thêm Key Vault.

→ Vậy bản ghi chú này chỉ để **biết cách làm**, không thay thế hạ tầng ACA đang dùng.

## Dọn dẹp sau khi thực hành xong
```bash
az group delete --name techstore-appservice-rg --yes
```
Resource group riêng nên xóa sạch không ảnh hưởng gì tới `techstore-rg` (ACR, Container Apps environment, container app ACA vẫn giữ nguyên).
