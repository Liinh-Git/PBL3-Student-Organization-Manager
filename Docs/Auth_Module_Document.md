# AUTH E2E GUIDE (BE -> FE)

## 1. Mục tiêu
Tài liệu này mô tả đầy đủ cách triển khai và vận hành tính năng xác thực end-to-end gồm:
- Register
- Login
- Me (lấy thông tin người dùng hiện tại)
- Logout ở phía frontend

Scope hiện tại:
- JWT Access Token
- Lưu token bằng localStorage ở frontend
- Google OAuth chỉ là UI placeholder

Ngoài scope hiện tại:
- Refresh token
- Forgot/reset password
- OAuth Google thật
- MFA

---

## 2. Kiến trúc đang áp dụng (VSA lai clean)

### 2.1 Backend (feature-first theo Vertical Slice)
Mỗi use-case auth nằm trong một endpoint riêng tại feature:
- `src/Org.Backend/Features/Auth/RegisterEndpoint.cs`
- `src/Org.Backend/Features/Auth/LoginEndpoint.cs`
- `src/Org.Backend/Features/Auth/MeEndpoint.cs`

Phần hạ tầng dùng chung (cross-cutting) nằm ở Infrastructure:
- `src/Org.Backend/Infrastructure/Auth/JwtTokenService.cs`
- `src/Org.Backend/Infrastructure/Auth/JwtOptions.cs`
- `src/Org.Backend/Infrastructure/Database/AppDbContext.cs`

### 2.2 Shared contracts (contract-first giữa FE và BE)
DTO giao tiếp đặt ở shared library để FE và BE cùng dùng chung kiểu dữ liệu:
- `src/Org.Shared/Features/Auth/AuthContracts.cs`

### 2.3 Frontend (UI + auth state)
Frontend tổ chức theo component + services:
- `src/Org.Frontend/Components/Pages/Auth/Login.razor`
- `src/Org.Frontend/Components/Pages/Auth/Register.razor`
- `src/Org.Frontend/Services/Auth/AuthApiClient.cs`
- `src/Org.Frontend/Services/Auth/FrontendAuthStateProvider.cs`
- `src/Org.Frontend/Services/Auth/LocalStorageTokenStorage.cs`

---

## 3. Cách BE và FE giao tiếp

## 3.1 Cơ chế gọi API
Frontend dùng typed `HttpClient` (`AuthApiClient`) để gọi backend.
Base URL được cấu hình tại:
- `src/Org.Frontend/appsettings.json`
- `src/Org.Frontend/appsettings.Development.json`

Ví dụ cấu hình:
```json
"BackendApi": {
  "BaseUrl": "http://localhost:5058"
}
```

## 3.2 CORS
Backend cho phép origin frontend qua policy `FrontendPolicy` tại:
- `src/Org.Backend/Program.cs`

Origins mặc định:
- `http://localhost:5236`
- `https://localhost:7112`

Khai báo trong:
- `src/Org.Backend/appsettings.json`

## 3.3 Truyền token
- Login thành công trả về `accessToken` + `expiresAtUtc`.
- FE lưu vào localStorage qua `LocalStorageTokenStorage`.
- Khi cần gọi endpoint bảo vệ (`/api/auth/me`), FE gắn header:
```http
Authorization: Bearer <access_token>
```

---

## 4. Hợp đồng dữ liệu (Data Contracts)
Contracts dùng chung nằm ở:
- `src/Org.Shared/Features/Auth/AuthContracts.cs`

## 4.1 Register
### Request
```json
{
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "password": "P@ssword123"
}
```
### Response (200)
```json
{
  "userId": "uuid",
  "fullName": "Nguyen Van A",
  "email": "user@example.com"
}
```

## 4.2 Login
### Request
```json
{
  "email": "user@example.com",
  "password": "P@ssword123"
}
```
### Response (200)
```json
{
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-03-31T10:00:00Z",
  "userId": "uuid",
  "fullName": "Nguyen Van A",
  "email": "user@example.com"
}
```

## 4.3 Me
### Request
```http
GET /api/auth/me
Authorization: Bearer <jwt>
```
### Response (200)
```json
{
  "userId": "uuid",
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "status": "Active"
}
```

## 4.4 Error mapping
- 400: dữ liệu không hợp lệ
- 401: sai thông tin đăng nhập hoặc token không hợp lệ/hết hạn
- 404: user không tồn tại khi gọi `me`
- 409: email đã tồn tại khi register

---

## 5. Luồng dữ liệu end-to-end

## 5.1 Register flow
1. User submit form ở `Register.razor`.
2. FE gọi `AuthApiClient.RegisterAsync(RegisterRequest)`.
3. BE `RegisterEndpoint` validate + hash password bằng BCrypt + lưu DB.
4. FE nhận `RegisterResponse`.
5. FE tự gọi login để auto sign-in.

## 5.2 Login flow
1. User submit form ở `Login.razor`.
2. FE gọi `AuthApiClient.LoginAsync(LoginRequest)`.
3. BE `LoginEndpoint` kiểm tra mật khẩu (BCrypt), cập nhật `LastLogin`, tạo JWT.
4. FE lưu token vào localStorage qua `LocalStorageTokenStorage`.
5. FE gọi `FrontendAuthStateProvider.SignInAsync(...)` để build `ClaimsPrincipal`.
6. Router cập nhật trạng thái authenticated và chuyển vào trang protected.

## 5.3 Session restore flow (khi reload trang)
1. `AuthBootstrapper` gọi `FrontendAuthStateProvider.InitializeAsync()`.
2. Provider đọc token trong localStorage.
3. Nếu token hợp lệ, gọi `/api/auth/me` để xác nhận và dựng claims.
4. Nếu token lỗi/hết hạn, tự clear token và set anonymous.

## 5.4 Logout flow
1. User bấm logout ở `NavMenu` hoặc top bar.
2. `FrontendAuthStateProvider.SignOutAsync()` clear localStorage.
3. Auth state về anonymous.
4. Điều hướng về `/login`.

---

## 6. Tổ chức thư mục theo trách nhiệm

## 6.1 Backend
```text
src/Org.Backend/
  Domain/
    Entities/                 # Entity model (User, Organization, ...)
    Enums/
  Features/
    Auth/
      RegisterEndpoint.cs     # Register slice
      LoginEndpoint.cs        # Login slice
      MeEndpoint.cs           # Me slice
  Infrastructure/
    Auth/
      IJwtTokenService.cs
      JwtTokenService.cs
      JwtOptions.cs
    Database/
      AppDbContext.cs
  Program.cs                  # DI + middleware + auth + CORS
```

## 6.2 Shared
```text
src/Org.Shared/
  Features/
    Auth/
      AuthContracts.cs        # DTO contract dùng chung FE/BE
```

## 6.3 Frontend
```text
src/Org.Frontend/
  Components/
    Auth/
      AuthBootstrapper.razor  # Khởi tạo auth state khi app start
      RedirectToLogin.razor   # Redirect khi truy cập route protected
    Layout/
      MainLayout.razor
      NavMenu.razor
      AuthLayout.razor        # Layout riêng cho login/register
    Pages/
      Auth/
        Login.razor
        Register.razor
      Home.razor              # Trang protected
  Services/
    Auth/
      AuthApiClient.cs
      AuthApiException.cs
      ITokenStorage.cs
      LocalStorageTokenStorage.cs
      FrontendAuthStateProvider.cs
  appsettings*.json           # BackendApi:BaseUrl
  Program.cs                  # DI auth + HttpClient + AuthorizationCore
```

---

## 7. Cách chạy local E2E

## 7.1 Chuẩn bị backend secrets
Tại backend project:
```bash
cd src/Org.Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=<YOUR_PASSWORD>"
dotnet user-secrets set "Jwt:SigningKey" "<RANDOM_SECRET_MIN_32_CHARS>"
```

## 7.2 Chạy backend và frontend
Terminal 1:
```bash
cd src/Org.Backend
dotnet run
```

Terminal 2:
```bash
cd src/Org.Frontend
dotnet run
```

## 7.3 Smoke test thủ công
1. Vào `/register` và tạo tài khoản.
2. Kiểm tra auto login và chuyển về `/`.
3. Refresh trình duyệt: vẫn đăng nhập do localStorage.
4. Vào `/counter`, `/weather`: truy cập được khi authenticated.
5. Logout và thử vào `/counter`: bị redirect về `/login`.

---

## 8. Sequence tổng quát (text)
```text
User -> FE Register Page -> AuthApiClient -> BE /api/auth/register -> DB
User -> FE Login Page -> AuthApiClient -> BE /api/auth/login -> JWT
FE -> localStorage save token
FE -> /api/auth/me (Bearer token) -> BE validate JWT -> DB -> MeResponse
FE -> FrontendAuthStateProvider -> ClaimsPrincipal -> protected routes unlocked
```

---

## 9. Lỗi thường gặp và cách xử lý

## 9.1 28P01 password authentication failed
Nguyên nhân: password PostgreSQL sai hoặc chưa đồng bộ user-secrets.

Cách xử lý:
```bash
cd src/Org.Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=<CORRECT_PASSWORD>"
```

## 9.2 CORS blocked trên browser
- Kiểm tra frontend origin có nằm trong `Cors:AllowedOrigins` của backend chưa.
- Đảm bảo backend đã `app.UseCors("FrontendPolicy")` trước auth middleware.

## 9.3 401 khi gọi /api/auth/me
- Token hết hạn hoặc sai SigningKey.
- Xóa localStorage token rồi login lại.

## 9.4 Redirect loop về /login
- Đảm bảo trang login/register có `AllowAnonymous`.
- Đảm bảo route protected có `[Authorize]` đúng mục tiêu.

## 9.5 FE build failed với MSB3027/MSB3021 (Org.Frontend.exe bị lock)
Triệu chứng:
- Cảnh báo MSB3026 lặp nhiều lần khi copy apphost.exe.
- Lỗi MSB3027 hoặc MSB3021 với thông báo file đang bị process Org.Frontend giữ.

Nguyên nhân:
- Đang có instance frontend cũ chưa tắt (thường do một terminal khác vẫn chạy dotnet run).

Cách xử lý:
```bash
# Đóng app đang chạy trong terminal hiện tại trước
# (Ctrl + C)

# Nếu vẫn bị lock, kill toàn bộ process frontend:
Get-Process -Name Org.Frontend -ErrorAction SilentlyContinue | Stop-Process -Force

# Chạy lại:
cd src/Org.Frontend
dotnet run
```

## 9.6 Failed to bind address 127.0.0.1:5236 (address already in use)
Nguyên nhân:
- Cổng 5236 đang bị chiếm bởi một instance frontend khác.

Cách xử lý:
```bash
# Tìm process chiếm cổng:
Get-NetTCPConnection -LocalPort 5236 -State Listen | Select-Object LocalAddress, LocalPort, OwningProcess

# Xem thông tin process:
Get-Process -Id <OwningProcess>

# Dừng process nếu cần:
Stop-Process -Id <OwningProcess> -Force
```

---

## 10. Nâng cấp đề xuất cho phase tiếp theo
1. Bổ sung refresh token + token rotation.
2. Chuyển từ localStorage sang HttpOnly cookie nếu muốn tăng mức bảo mật.
3. Thêm rate limiting cho login/register.
4. Thêm audit log cho sự kiện đăng nhập thất bại/thành công.
5. Bổ sung test integration cho auth endpoints.
