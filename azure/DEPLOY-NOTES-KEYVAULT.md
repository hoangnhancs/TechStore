# Ghi chú: tích hợp Azure Key Vault cho ProductService (local + ACA)

Mục tiêu: chuyển secret (Cloudinary, DB, JWT, RabbitMQ) từ `.env`/config thường sang đọc từ Azure Key Vault — làm local trước, sau đó áp dụng tiếp cho Container App trên ACA.

---

## PHẦN 1 — Code (1 lần, dùng chung cho cả local lẫn ACA)

### 1.1. Package
```bash
dotnet add server/ProductService/ProductService.csproj package Azure.Identity
dotnet add server/ProductService/ProductService.csproj package Azure.Extensions.AspNetCore.Configuration.Secrets
```

### 1.2. [`Program.cs`](../server/ProductService/Program.cs) — thêm ngay sau khi tạo `builder`
```csharp
using Azure.Identity;
...
var keyVaultUri = builder.Configuration["KeyVaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}
```
Để trống `KeyVaultUri` → không bật Key Vault, hành vi y như cũ (đọc `.env`/appsettings). Set URL → Key Vault override giá trị cùng key, vì `AddAzureKeyVault` được add **sau cùng** trong chuỗi configuration provider (last-added wins).

### 1.3. Quy ước đặt tên secret
Key Vault không cho dùng `:` trong tên secret → dùng `--` thay thế, `KeyVaultSecretManager` mặc định tự convert ngược lại thành `:` khi đọc. Ví dụ `CloudinarySettings:ApiKey` (config key) ↔ `CloudinarySettings--ApiKey` (tên secret trên Key Vault).

---

## PHẦN 2 — Local: tạo Key Vault + chạy thử

### 2.1. Tạo Key Vault + set secret
```bash
az keyvault create --name techstore-kv-demo --resource-group testRG --location southeastasia
az keyvault secret set --vault-name techstore-kv-demo --name "ConnectionStrings--DefaultConnection" --value "<db-conn-string>"
az keyvault secret set --vault-name techstore-kv-demo --name "Jwt--Key" --value "<jwt-key>"
az keyvault secret set --vault-name techstore-kv-demo --name "RabbitMq--ConnectionString" --value "<rabbitmq-conn>"
az keyvault secret set --vault-name techstore-kv-demo --name "CloudinarySettings--CloudName" --value "<cloud-name>"
az keyvault secret set --vault-name techstore-kv-demo --name "CloudinarySettings--ApiKey" --value "<api-key>"
az keyvault secret set --vault-name techstore-kv-demo --name "CloudinarySettings--ApiSecret" --value "<api-secret>"
```

⚠️ **Lỗi gặp #1 — `403 Forbidden` (`ForbiddenByRbac`) khi chạy `secret set`**: role `Key Vault Secrets User` chỉ là **read-only** (get/list), không có quyền `set`/`delete`. Fix — gán thêm role `Key Vault Secrets Officer` (read + write), giữ nguyên `Key Vault Secrets User` cho runtime đọc (2 role không xung đột nhau):
```bash
az role assignment create --assignee <your-user-object-id-or-upn> --role "Key Vault Secrets Officer" --scope "/subscriptions/<sub-id>/resourceGroups/testRG/providers/Microsoft.KeyVault/vaults/techstore-kv-demo"
```

### 2.2. Config local — `.env`
```
KeyVaultUri=https://techstore-kv-demo.vault.azure.net/
```
`DefaultAzureCredential` tự dùng session `az login` hiện có trên máy — không cần setup gì thêm cho local.

### 2.3. Test override hoạt động
Comment hết các dòng secret gốc trong `.env` (Cloudinary, giữ `KeyVaultUri`), chạy `dotnet run`, gọi API upload banner — nếu vẫn ra ảnh thật trên Cloudinary nghĩa là app đang đọc đúng từ Key Vault, không phải từ `.env`.

⚠️ **Lỗi gặp #2 — upload banner 500 `"Missing required parameter - api_key"`**: comment `.env` nhưng **quên thêm dòng `KeyVaultUri=`** → điều kiện `if (!string.IsNullOrEmpty(keyVaultUri))` fail → Key Vault không được add → Cloudinary config rỗng. Fix: thêm lại dòng `KeyVaultUri=...` vào `.env`.

---

## PHẦN 3 — ACA: managed identity + đọc Key Vault trên cloud

### 3.1. Vì sao cần managed identity (khác local)
Local dùng session `az login` cá nhân để `DefaultAzureCredential` xác thực. Container App chạy trên cloud không có khái niệm "đăng nhập cá nhân" — cần 1 **managed identity** riêng để tự xác thực lúc runtime.

### 3.2. Gán identity + cấp quyền
```bash
az containerapp identity assign --name product-svc --resource-group techstore-rg --system-assigned
```
```powershell
$PRINCIPAL_ID = az containerapp identity show --name product-svc --resource-group techstore-rg --query principalId -o tsv
$KV_ID = az keyvault show --name techstore-kv-demo --resource-group testRG --query id -o tsv
az role assignment create --assignee $PRINCIPAL_ID --role "Key Vault Secrets User" --scope $KV_ID
```

### 3.3. Config qua YAML — [`containerapp-product-svc.yaml`](../azure/containerapp-product-svc.yaml)
Thay vì dùng field `keyVaultUrl:`/`identity:` riêng của ACA cho từng secret, chọn cách **tái dùng đúng code path đã có** (`AddAzureKeyVault` trong `Program.cs`) — chỉ cần thêm 1 env var, đơn giản hơn và đồng nhất dev/prod:
```yaml
env:
  # ...giữ nguyên các env khác...
  # - name: CloudinarySettings__CloudName   (comment hết Cloudinary — để Key Vault cung cấp)
  #   value: ...
  - name: KeyVaultUri
    value: https://techstore-kv-demo.vault.azure.net/
```
Áp dụng:
```bash
az containerapp update --name product-svc --resource-group techstore-rg --yaml azure/containerapp-product-svc.yaml
```
Lưu ý: `--yaml` là **full replace** phần `template`/`secrets` — field nào thiếu trong YAML sẽ bị xoá khỏi Container App thật, khác với `--image` (chỉ patch đúng field `image`, không đụng env/secrets).

---

## PHẦN 4 — Bug gặp phải khi test trên ACA (và cách tìm ra)

### 4.1. Thiếu managed identity → **crash toàn bộ app lúc start**
Test lúc cố tình **chưa** gán managed identity (xem [Phần 3.2](#32-gán-identity--cấp-quyền) — làm sau, không phải trước) để quan sát hành vi thật.

**Triệu chứng gây nhầm lẫn**: gọi API vẫn ra `401 Unauthorized` (không phải lỗi 500 như kỳ vọng) — vì Container Apps **Single revision mode** tự giữ nguyên revision cũ đang healthy phục vụ traffic khi revision mới không pass health check, "che" mất lỗi thật.

**Cách tìm ra nguyên nhân thật — check thẳng revision mới nhất, không qua domain chính**:
```bash
az containerapp revision list --name product-svc --resource-group techstore-rg \
  --query "[].{name:name, created:properties.createdTime}" -o table

az containerapp revision show --name product-svc --resource-group techstore-rg \
  --revision <tên-revision-mới-nhất> \
  --query "{health:properties.healthState, running:properties.runningState}" -o json
```
→ `"health": "Unhealthy", "running": "ActivationFailed"` — xác nhận revision mới thật sự chết, không phải do code logic sai.

**Xem log chi tiết** — `az containerapp logs show` chỉ tail log từ **replica đang sống**, revision đã crash (`replicas: 0`) thì báo `Could not find a replica for this app`, không dùng được. Phải query qua **Log Analytics** (lưu log cũ dù container đã chết):
```bash
az extension add --name log-analytics --yes

WORKSPACE_ID=$(az containerapp env show --name techstore-env --resource-group techstore-rg \
  --query "properties.appLogsConfiguration.logAnalyticsConfiguration.customerId" -o tsv)

az monitor log-analytics query --workspace "$WORKSPACE_ID" \
  --analytics-query "ContainerAppConsoleLogs_CL | where RevisionName_s == '<tên-revision>' | order by TimeGenerated asc | project TimeGenerated, Log_s | take 30" \
  -o table
```
→ log ra đúng dòng gốc:
```
Unhandled exception. Azure.Identity.CredentialUnavailableException: DefaultAzureCredential failed to retrieve a token...
- ManagedIdentityCredential authentication unavailable. No Managed Identity endpoint found.
```
Xác nhận 100%: thiếu managed identity → `AddAzureKeyVault` throw exception chưa catch lúc `Program.cs` chạy → **crash cả app**, không phải lỗi riêng ở 1 API nào.

**Fix**: chạy lại đúng 2 lệnh ở [Phần 3.2](#32-gán-identity--cấp-quyền).

### 4.2. Revision `ActivationFailed` không tự retry được bằng `revision restart`
Gán xong managed identity, dùng `az containerapp revision restart` để ép container chạy lại — **không ăn thua**, vẫn `Unhealthy`, log không có dòng crash mới (do revision đã ở trạng thái `ActivationFailed` ngay từ đầu, lệnh `restart` chỉ thiết kế cho revision **đã từng chạy ổn rồi mới crash sau đó**, không áp dụng được cho revision chưa từng activate thành công lần nào).

**Fix** — ép tạo hẳn 1 revision mới (dù code/image không đổi), dùng `--revision-suffix`:
```bash
az containerapp update --name product-svc --resource-group techstore-rg --revision-suffix retry1
```
Revision mới luôn đọc lại managed identity + Key Vault từ đầu, không bị kẹt trạng thái cũ. Kết quả: `"health": "Healthy", "running": "Running"`.

### 4.3. Thiếu secret `CloudinarySettings--CloudName` trên Key Vault
App hết crash, nhưng upload banner vẫn lỗi `500 — "Cloud name must be specified in Account!"`. Check lại:
```bash
az keyvault secret list --vault-name techstore-kv-demo --query "[].name" -o tsv
```
→ chỉ thấy `CloudinarySettings--ApiKey`/`ApiSecret`, **thiếu hẳn `CloudinarySettings--CloudName`** (sót từ lúc set secret ban đầu). Fix:
```bash
az keyvault secret set --vault-name techstore-kv-demo --name "CloudinarySettings--CloudName" --value "<cloud-name>"
```
**Lưu ý quan trọng**: secret Key Vault chỉ được đọc **1 lần lúc app start** (`AddAzureKeyVault` chạy trong `Program.cs`, trước `app.Run()`) — thêm secret mới xong **không tự áp dụng** cho revision đang chạy, phải ép tạo revision mới (`--revision-suffix`) như 4.2 thì app mới đọc lại Key Vault với secret vừa thêm.

### 4.4. CI/CD báo "thành công" dù app thật sự đang crash
`az containerapp update --image ...` trong `deploy-product-svc.yml` chỉ chờ tới khi **ARM xác nhận đã nhận request tạo revision mới** — không chờ container thực sự chạy được. Cộng với hành vi Single revision mode ở mục 4.1 (giữ revision cũ phục vụ traffic khi revision mới fail), pipeline báo xanh (exit 0) dù revision mới crash hoàn toàn.

**Fix — thêm step verify health sau khi update, check thẳng vào revision vừa tạo** (không gọi qua domain chính, vì domain chính luôn trả 200 từ revision cũ):
```yaml
- name: Verify revision mới thực sự healthy
  run: |
    REVISION=$(az containerapp revision list \
      --name ${{ env.CONTAINER_APP_NAME }} \
      --resource-group ${{ env.RESOURCE_GROUP }} \
      --query "max_by([], &properties.createdTime).name" -o tsv)
    for i in $(seq 1 10); do
      HEALTH=$(az containerapp revision show \
        --name ${{ env.CONTAINER_APP_NAME }} \
        --resource-group ${{ env.RESOURCE_GROUP }} \
        --revision "$REVISION" --query properties.healthState -o tsv)
      if [ "$HEALTH" = "Healthy" ]; then exit 0; fi
      sleep 10
    done
    echo "::error::Revision $REVISION không healthy sau khi chờ"
    exit 1
```
⚠️ Lỗi nhỏ gặp lúc viết: `--query "max_by([], &createdTime).name"` sai field path → `createdTime` thực ra nằm trong `properties.createdTime`, không phải field gốc — JMESPath trả `null` cho mọi phần tử, `max_by` lỗi `invalid type for value: None`. Fix: `&properties.createdTime`.

---

## PHẦN 5 — Cheat sheet: check sức khỏe service trên ACA

| Việc cần làm | CLI | Portal |
|---|---|---|
| Xem revision nào đang Healthy | `az containerapp revision list --query "[].{name:name,health:properties.healthState}" -o table` | Container App → **Revisions and replicas** |
| Xem chi tiết 1 revision | `az containerapp revision show --revision <tên>` | Click vào tên revision → panel **Revision details** |
| Log container đang sống | `az containerapp logs show --name <app> -g <rg> --revision <tên> --tail 50` | **Log stream** (sidebar) |
| Log container đã crash (không còn replica) | `az monitor log-analytics query --workspace <id> --analytics-query "ContainerAppConsoleLogs_CL | where RevisionName_s == '<tên>' | ..."` | **Logs** (sidebar, viết KQL trực tiếp) |
| Danh sách secret trong Key Vault | `az keyvault secret list --vault-name <kv> --query "[].name" -o tsv` | Key Vault → **Secrets** |
| Managed identity hiện có của app chưa | `az containerapp identity show --name <app> -g <rg>` | Container App → **Identity** |

---

## Còn tồn đọng
- `BannerController.cs` hiện đang tạm `[AllowAnonymous]` để test — cần đổi lại `[Authorize(Roles = "Admin")]` và push lại trước khi coi là xong việc.
- Chưa deploy `identity-svc` lên ACA — muốn test full flow Admin login thật qua cloud (thay vì `AllowAnonymous` tạm) thì cần làm thêm bước này riêng.
