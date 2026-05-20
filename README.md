# PBL3 - Hệ thống quản lý tổ chức sinh viên

Repo này là hệ thống quản lý tổ chức sinh viên với backend .NET và frontend React.

## Trạng thái hiện tại

- Backend đã có nhiều module nghiệp vụ với endpoint, service, validator và mapping thực thi.
- Frontend đã có auth, workspace routing và tích hợp phần lớn API chính.
- Một số luồng vẫn còn TODO, đặc biệt nhóm `Discover` và một phần permissions nâng cao.
- Tài liệu snapshot kỹ thuật chi tiết: [docs/SUMMARY.md](docs/SUMMARY.md)

## Cấu trúc repo

```text
PBL3/
  backend/
    Org.Backend/      # API backend (FastEndpoints + EF Core + JWT)
    Org.Shared/       # Shared contracts/DTO/enums dùng chung
  frontend/           # React + Vite
  docs/               # Tài liệu dự án
  tests/              # Hiện chưa có test .cs thực thi trong snapshot này
  StudentOrgManager.slnx
```

## Công nghệ chính

- Backend:
  - .NET 10
  - FastEndpoints
  - EF Core + PostgreSQL
  - JWT Bearer Authentication
- Frontend:
  - React 18 + Vite 5
  - React Router v6
  - Axios

## Yêu cầu môi trường

- .NET SDK 10
- Node.js 18+
- PostgreSQL

## Chạy backend

1. Cập nhật connection string trong `backend/Org.Backend/appsettings.json` hoặc dùng biến môi trường tương đương.
2. Cung cấp các cấu hình JWT bắt buộc:
   - `Jwt:SigningKey`
   - `Jwt:Issuer`
   - `Jwt:Audience`
3. Chạy:

```bash
dotnet restore StudentOrgManager.slnx
dotnet build StudentOrgManager.slnx
dotnet run --project backend/Org.Backend/Org.Backend.csproj
```

- Mặc định backend chạy ở `http://localhost:5000`.
- Swagger được bật trong môi trường Development.

## Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

- Mặc định Vite chạy ở cổng `3000`.
- API base URL dùng biến `VITE_API_BASE_URL`:
  - Nếu không khai báo, frontend sẽ dùng mặc định `http://localhost:5000/api`.

Ví dụ `.env` trong `frontend/`:

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

## Lưu ý quan trọng

- `orgId` trong org workspace lấy từ query string `?orgId=...`, không lấy từ path params.
- `403` ở các route kiểm tra quyền thành viên là tình huống hợp lệ, không phải auto logout.
- Có một số trang prototype placeholder:
  - `/org/tasks`
  - `/org/resources`
  - `/org/reports`
  - `/org/finance`

## Tài liệu liên quan

- [Tổng quan hiện trạng repo](docs/SUMMARY.md)
