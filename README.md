# TÀI LIỆU HƯỚNG DẪN CÀI ĐẶT VÀ VẬN HÀNH DỰ ÁN

## 1. Yêu cầu tiên quyết về môi trường phát triển

Để đảm bảo tính đồng nhất trong quá trình phát triển và tránh xung đột môi trường, các thành viên trong nhóm dự án yêu cầu phải cài đặt các công cụ phần mềm sau:

- .NET SDK: Phiên bản 10.0 hoặc mới hơn (https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
- PostgreSQL: Cài đặt bản local để phục vụ phát triển hằng ngày (https://www.postgresql.org/download/).
- Docker Desktop: Chỉ dùng ở bước triển khai/kiểm thử cuối cùng (không bắt buộc trong quá trình dev hằng ngày).
- Môi trường phát triển tích hợp (IDE):
  - Đối với hệ điều hành Windows: Visual Studio 2022 hoặc JetBrains Rider.
  - Đối với hệ điều hành Linux/macOS: JetBrains Rider hoặc Visual Studio Code (cài đặt tiện ích mở rộng C# Dev Kit).

## 2. Quy trình khởi tạo mã nguồn và cơ sở dữ liệu

Bước 2.1: Sao chép mã nguồn
Mở giao diện dòng lệnh (Terminal/PowerShell) tại thư mục lưu trữ dự án cục bộ và thực thi lệnh sau:

`git clone <đường-dẫn-github-của-repository>`
`cd StudentOrgManager`

Bước 2.2: Cấu hình Connection String bằng C# User Secrets (khuyến nghị)
Tại thư mục dự án Backend, thiết lập connection string qua user-secrets để tránh lưu mật khẩu trong repo:

cd src/Org.Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=CHANGE_ME"

*Ghi chú: Docker chỉ dùng ở bước triển khai/kiểm thử cuối cùng, không bắt buộc trong quá trình dev hằng ngày.*

## 3. Hướng dẫn khởi chạy hệ thống

Hệ thống được thiết kế bao gồm hai phân hệ chính: Backend (Application Programming Interface) và Frontend (User Interface). Cần khởi chạy đồng thời cả hai phân hệ này để ứng dụng hoạt động hoàn chỉnh.

Phương pháp 1: Khởi chạy thông qua Giao diện dòng lệnh (CLI) - Nên dùng
Sử dụng hai cửa sổ Terminal độc lập:
- Terminal 1 (Khởi chạy Backend):
  `cd src/Org.Backend`
  `dotnet run`

- Terminal 2 (Khởi chạy Frontend):
  `cd src/Org.Frontend`
  `dotnet run`

Phương pháp 2: Khởi chạy thông qua Visual Studio 2022 (Dành cho Windows)
1. Mở tệp giải pháp StudentOrgManager.sln.
2. Nhấp chuột phải vào tên Solution trong cửa sổ Solution Explorer, chọn "Configure Startup Projects...".
3. Thiết lập tùy chọn "Multiple startup projects".
4. Thay đổi thuộc tính Action của hai dự án Org.Backend và Org.Frontend thành "Start".
5. Nhấn phím F5 hoặc nút Start để tiến hành biên dịch và khởi chạy hệ thống.

## 4. Nguyên tắc kiến trúc và tổ chức mã nguồn

Dự án này áp dụng mô hình Kiến trúc Cắt lát dọc Thực dụng (Pragmatic Vertical Slice Architecture), thay thế cho mô hình MVC truyền thống. Nhóm cần tuân thủ nghiêm ngặt các nguyên tắc sau:

- Tầng Thực thể (Domain Entities): Toàn bộ các lớp đại diện cho cấu trúc bảng cơ sở dữ liệu (Database Schema) được quản lý tập trung tại thư mục src/Org.Backend/Domain/Entities/.
- Tầng Nghiệp vụ (Features): Mã nguồn xử lý logic được phân chia theo từng chức năng cụ thể (Use Cases). Khi phát triển một tính năng mới, lập trình viên tạo tệp Endpoint (hoặc Handler) trực tiếp tại thư mục của phân hệ đó (Ví dụ: src/Org.Backend/Features/Tasks/). Khuyến cáo không khởi tạo các lớp Controller hay Service phân tán bên ngoài định dạng phân hệ chức năng.
- Tầng Giao tiếp (Shared Contracts): Các đối tượng truyền tải dữ liệu (Data Transfer Objects - DTOs) dùng để giao tiếp giữa Frontend và Backend phải được định nghĩa tại src/Org.Shared/Features/ nhằm đảm bảo tính đồng bộ và an toàn kiểu dữ liệu.

## 5. Docker (chỉ dùng ở bước cuối)

Khi cần triển khai/kiểm thử cuối cùng bằng Docker, dùng tệp docker-compose.yml tại thư mục gốc:

docker compose up -d

Để dừng dịch vụ:

docker compose down
