# Hướng dẫn Đồng bộ Database và Dữ liệu mẫu (Seeding)

Tài liệu này hướng dẫn cách cập nhật cấu trúc database (Migration) và nạp 20 bản ghi mẫu cho mỗi thực thể để phục vụ việc phát triển local.

---

### 1. Điều kiện tiên quyết
- Đã cài đặt **.NET 10 SDK**.
- Đã cài đặt **PostgreSQL** và cấu hình `ConnectionStrings:DefaultConnection` bằng file `.env` ở thư mục gốc repo (không commit). Có thể tạo nhanh bằng:
   ```powershell
   cp .env.example .env
   ```
   Sau đó sửa `ConnectionStrings__DefaultConnection` trong `.env` theo database local của bạn.
- Đã cài đặt `dotnet-ef` tool:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

### 2. Cách chạy nhanh (Khuyên dùng)
Tôi đã tạo sẵn script PowerShell để thực hiện 2 việc: Update DB và Seed dữ liệu chỉ với 1 câu lệnh.

**Câu lệnh:**
```powershell
./scripts/dev/sync-db.ps1
```

**Script này sẽ tự động:**
1. Di chuyển vào thư mục `src/Org.Backend`.
2. Chạy `dotnet ef database update` để cập nhật schema mới nhất (bao gồm `EventCategory`).
3. Chạy `dotnet run -- --seed` để nạp 20 bản ghi mẫu (Users, Orgs, Categories, Tasks, Finance...).

---

### 3. Cách chạy thủ công (Nếu script lỗi)
Nếu bạn không muốn dùng script, hãy chạy các lệnh sau theo thứ tự:

1. **Cập nhật Database:**
   ```powershell
   cd src/Org.Backend
   dotnet ef database update
   ```

2. **Nạp dữ liệu mẫu (Seed Mode):**
   ```powershell
   dotnet run -- --seed
   ```
   *Lưu ý: Chế độ `--seed` sẽ tự động thoát sau khi hoàn tất nạp dữ liệu.*

---

### 4. Kiểm tra dữ liệu
Sau khi chạy thành công, bạn sẽ thấy log thông báo số lượng bản ghi đã tạo. Ví dụ:
- **20 Users** (nếu có đăng ký x user thì là x + 20 mẫu).
- **20 Organizations**.
- **20 Event Categories** (Cấu trúc phân cấp).
- **20 Tasks / Milestones**.

---

### 5. Lưu ý cho mọi người
- **Cấu trúc Program.cs mới:** Logic khởi tạo đã được tách ra các Extension Methods trong `Infrastructure/Startup` để tránh xung đột code (Merge Conflict) sau này.
- **Tính Idempotent:** Có thể chạy lệnh Seed nhiều lần. Nếu dữ liệu đã tồn tại, hệ thống sẽ bỏ qua và không tạo trùng lặp.
- **Vietnamese Comments:** Toàn bộ file cấu trúc quan trọng đã được comment tiếng Việt chi tiết để mọi người dễ nắm bắt logic.
