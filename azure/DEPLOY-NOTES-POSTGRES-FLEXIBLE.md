# Ghi chú: thực hành tạo Azure Database for PostgreSQL Flexible Server, migrate + seed ProductService

Ghi lại buổi thực hành thử đổi database của ProductService sang 1 Postgres chạy trên Azure (thay vì Neon) — mục đích chỉ để biết cách làm, **không** giữ lại lâu dài. Resource đã được xoá sạch ở cuối bài.

---

## 0. Điểm xuất phát — vì sao không dùng Azure SQL

Trước đó đã lỡ tạo 1 **Azure SQL Database** (`techstore-sql-2026-nhan-01`, database con tên `product`, nằm trong resource group `testRG`). Kiểm tra lại mới phát hiện: ProductService dùng `UseNpgsql` ([Program.cs:20](../server/ProductService/Program.cs#L20)) — provider cho PostgreSQL, **không tương thích** với Azure SQL (SQL Server):
- Package `Npgsql.EntityFrameworkCore.PostgreSQL` không connect được tới SQL Server.
- 6 migration sẵn có được generate theo cú pháp Postgres — chạy thẳng lên SQL Server sẽ lỗi.

Đổi provider trong code là 1 thay đổi thật (sửa `Program.cs`, đổi NuGet package, generate lại toàn bộ migrations) — không làm trong buổi thực hành này. Quyết định: **giữ nguyên code**, tạo 1 Postgres khác trên Azure thay vì SQL. (Azure SQL cũ vẫn còn nằm trong `testRG`, chưa xoá — xem phần cuối.)

## 1. Chuẩn bị — lấy IP public
Postgres Flexible Server cần khai báo rõ IP nào được phép connect (firewall), nên lấy IP public trước:
```bash
curl -s https://api.ipify.org
```
→ `113.161.80.58` (dùng ở bước tạo server).

## 2. Đăng ký resource provider
```bash
az provider show --namespace Microsoft.DBforPostgreSQL --query registrationState -o tsv
```
Ra `NotRegistered` → đăng ký (bài học cũ từ ACR: luôn kèm `--wait`, thiếu sẽ khiến bước sau chạy trước khi đăng ký xong mà không báo lỗi rõ ràng):
```bash
az provider register --namespace Microsoft.DBforPostgreSQL --wait
```

## 3. Tạo Postgres Flexible Server
```powershell
az postgres flexible-server create `
  --resource-group testRG `
  --name techstore-pg-demo `
  --location southeastasia `
  --admin-user pgadmin `
  --admin-password "<password>" `
  --sku-name Standard_B1ms `
  --tier Burstable `
  --storage-size 32 `
  --version 16 `
  --public-access 113.161.80.58
```
Chọn tier `Burstable B1ms` (rẻ nhất) theo đúng tư duy tiết kiệm compute đã áp dụng với Neon trước đây. `--public-access <IP>` tự mở firewall **chỉ cho đúng IP đó**, không mở toang ra cả internet.

⚠️ **Lỗi gặp**: copy nguyên lệnh bash (dùng `\` để nối dòng) paste vào PowerShell → `Missing expression after unary operator '--'`. PowerShell dùng backtick `` ` `` để nối dòng, không phải `\`. Fix bằng cách đổi `\` → `` ` ``, hoặc gõ gọn cả lệnh trên 1 dòng cho an toàn.

## 4. Tạo database `product` trong server
```bash
az postgres flexible-server db create --resource-group testRG --server-name techstore-pg-demo --name product
```
⚠️ **Lỗi gặp**: lúc đầu dùng `--database-name` (đoán theo tên flag ở lệnh `show-connection-string`) → báo thiếu `--name/-n`. Lệnh `db create` dùng flag `--name`, khác với `show-connection-string` dùng `--database-name` — 2 lệnh con khác nhau, flag không đồng nhất, phải nhớ đúng từng lệnh.

## 5. Vì sao Portal không thấy database (khác hẳn Azure SQL)
Vào thẳng `testRG` trên portal → chỉ thấy **server** `techstore-pg-demo`, không thấy database `product` — khác hẳn với Azure SQL lúc trước (`techstore-sql-2026-nhan-01` và `.../product` hiện ngang hàng nhau trong resource group).

Lý do là khác biệt thật ở resource model:
- **Azure SQL**: mỗi database là **1 resource ARM độc lập** (`Microsoft.Sql/servers/databases`) — có billing/scale riêng, nên hiện ngang hàng server trong resource group.
- **Postgres Flexible Server**: chỉ **server** là resource ARM/billable. Database bên trong là khái niệm nội bộ của chính Postgres (giống hệt tự cài Postgres, 1 server chứa nhiều database) — Azure không track riêng từng cái. Portal không liệt nó ở resource group; phải vào đúng trang server → menu **Settings → Databases** mới thấy, hoặc CLI:
  ```bash
  az postgres flexible-server db list --resource-group testRG --server-name techstore-pg-demo -o table
  ```

## 6. Lấy connection string
```bash
az postgres flexible-server show-connection-string --server-name techstore-pg-demo --database-name product --admin-user pgadmin --admin-password "<password>" --query npgsql -o tsv
```
⚠️ **Lỗi gặp**: output rỗng, không có gì — vì field `npgsql` **không tồn tại** trong JSON trả về (đoán sai tên field). Bỏ `--query`/`-o tsv` chạy lại để xem cấu trúc JSON thật, thấy các key có sẵn: `ado.net`, `jdbc`, `jdbc Spring`, `node.js`, `php`, `psql_cmd`, `python`, `ruby` — không có key tên "npgsql".

Dùng format `ado.net` (tương thích Npgsql), đổi theo đúng convention project hay dùng (`Host=`, `Username=` thay vì `Server=`, `User Id=`):
```
Host=techstore-pg-demo.postgres.database.azure.com;Database=product;Port=5432;Username=pgadmin;Password=<password>;Ssl Mode=Require;Trust Server Certificate=true
```

## 7. Chạy migration lên Postgres Azure
```bash
cd server/ProductService
dotnet ef database update --connection "<connection string ở bước 6>"
```
Áp toàn bộ 6 migration hiện có (`InitDb` → `Add product index updatedat`) lên database `product` mới — chạy thành công.

## 8. Xem table đã tạo — Portal vs CLI
**Portal**: Postgres Flexible Server **không có** Query editor sẵn như Azure SQL — phải dùng tool ngoài (VS Code extension PostgreSQL, pgAdmin, DBeaver...) để xem table trực quan.

**CLI** (không cần cài gì thêm ngoài 1 extension):
```bash
az extension add --name rdbms-connect
az postgres flexible-server execute --name techstore-pg-demo --admin-user pgadmin --admin-password "<password>" --database-name product --querytext "SELECT table_name FROM information_schema.tables WHERE table_schema='public';"
```
Hoặc nếu máy có sẵn `psql`:
```bash
psql "host=techstore-pg-demo.postgres.database.azure.com port=5432 dbname=product user=pgadmin password=<password> sslmode=require"
```
rồi gõ `\dt` trong psql.

## 9. Seed data — đổi connection string rồi start service
ProductService load connection string từ file **`.env`** (qua `DotNetEnv.Env.Load()` ở đầu [Program.cs](../server/ProductService/Program.cs), **không phải** `appsettings.Development.json`). Đổi dòng `ConnectionStrings__DefaultConnection` trong `.env` sang connection string Postgres Azure, rồi:
```bash
dotnet run --project server/ProductService
```
Program.cs có sẵn block auto-migrate + seed ([Program.cs:68-80](../server/ProductService/Program.cs#L68-L80)):
```csharp
await context.Database.MigrateAsync();
await DbInitializer.SeedData(context, logger);
```
Migration idempotent (đã áp ở bước 7 nên không làm gì thêm), seed thì chèn data mẫu nếu DB đang trống.

⚠️ **Lỗi gặp (transient)**: lần chạy đầu bị
```
Npgsql.NpgsqlException: Exception while reading from stream
---> An existing connection was forcibly closed by the remote host.
```
Lỗi xảy ra **sau khi migrate xong**, gãy ngay trong câu query đầu tiên của `SeedData`. Đây là lỗi transient (chập chờn mạng/kết nối đầu tiên tới server Postgres vừa mới tạo xong, server có thể còn đang warm-up) — không phải lỗi cấu hình. Chạy lại `dotnet run` lần 2 là qua, không cần sửa gì. (Nếu vẫn lặp lại nhiều lần, hướng debug tiếp: `az postgres flexible-server show --query state` xem server đã `Ready` chưa, và `az postgres flexible-server firewall-rule list` xem IP còn đúng không — phòng trường hợp IP mạng đổi.)

## 10. Dọn dẹp sau khi thực hành xong
Xoá server Postgres (xoá cả server, không chỉ database, vì server mới là đơn vị tính tiền):
```bash
az postgres flexible-server delete --resource-group testRG --name techstore-pg-demo --yes
```
Verify:
```bash
az postgres flexible-server list --resource-group testRG -o table   # trống = xoá xong
az resource list --resource-group testRG -o table                   # chỉ còn SQL server cũ
```

⚠️ **Phải nhớ trả `.env` về lại Neon** sau khi xoá server — nếu không, `ConnectionStrings__DefaultConnection` trong `.env` vẫn trỏ tới server đã xoá, lần chạy `dotnet run` tiếp theo sẽ fail kết nối ngay từ bước migrate. Đổi lại dòng 1 của `.env` về đúng connection string Neon gốc (xem giá trị thật trong `.env` cũ hoặc password manager — không chép lại ở đây vì file này có track git).

---

## Còn sót lại — chưa dọn
Resource group `testRG` **vẫn còn** Azure SQL Server `techstore-sql-2026-nhan-01` (+ database `master`, `product`) tạo từ đầu buổi — chưa dùng tới, chưa xoá. Vẫn đang tính tiền (SQL Database DTU/vCore luôn chạy, không tự scale-to-zero). Muốn dọn luôn:
```bash
az sql server delete --resource-group testRG --name techstore-sql-2026-nhan-01 --yes
```
Hoặc xoá sạch cả `testRG` 1 lần nếu không còn gì cần giữ trong đó:
```bash
az group delete --name testRG --yes
```
