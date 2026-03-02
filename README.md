# TÀI LIỆU HƯỚNG DẪN CÀI ĐẶT VÀ VẬN HÀNH DỰ ÁN

## 1. Yêu cầu tiên quyết về môi trường phát triển

Để đảm bảo tính đồng nhất trong quá trình phát triển và tránh xung đột môi trường, các thành viên trong nhóm dự án yêu cầu phải cài đặt các công cụ phần mềm sau:

- .NET SDK: Phiên bản 8.0 hoặc mới hơn.
- Docker Desktop: Công cụ ảo hóa bắt buộc để triển khai cơ sở dữ liệu dùng chung. Đối với người dùng hệ điều hành Windows, yêu cầu tích hợp tính năng Windows Subsystem for Linux (WSL 2) trong quá trình cài đặt.
- Môi trường phát triển tích hợp (IDE):
  - Đối với hệ điều hành Windows: Visual Studio 2022 hoặc JetBrains Rider.
  - Đối với hệ điều hành Linux/macOS: JetBrains Rider hoặc Visual Studio Code (cài đặt tiện ích mở rộng C# Dev Kit).

## 2. Quy trình khởi tạo mã nguồn và cơ sở dữ liệu

Bước 2.1: Sao chép mã nguồn
Mở giao diện dòng lệnh (Terminal/PowerShell) tại thư mục lưu trữ dự án cục bộ và thực thi lệnh sau:

`git clone <đường-dẫn-github-của-repository>`
`cd StudentOrgManager`

Bước 2.2: Triển khai cơ sở dữ liệu qua Docker
Đảm bảo dịch vụ Docker Desktop đang hoạt động trên hệ thống (chạy ngầm). Tại thư mục gốc của dự án (nơi chứa tệp cấu hình docker-compose.yml), thực thi lệnh:

`docker compose up -d`

*Ghi chú: Lệnh này sẽ tự động tải cấu trúc ảnh (image) và khởi tạo một máy chủ cơ sở dữ liệu PostgreSQL ảo. Các thông số về cổng kết nối (port), tài khoản và mật khẩu đã được định nghĩa sẵn trong tệp cấu hình để đảm bảo tính nhất quán cho toàn bộ nhóm.*

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

Dự án này áp dụng mô hình Kiến trúc Cắt lát dọc Thực dụng (Pragmatic Vertical Slice Architecture), thay thế cho mô hình N-Tier hoặc MVC truyền thống. Nhóm phát triển cần tuân thủ nghiêm ngặt các nguyên tắc sau:

- Tầng Thực thể (Domain Entities): Toàn bộ các lớp đại diện cho cấu trúc bảng cơ sở dữ liệu (Database Schema) được quản lý tập trung tại thư mục src/Org.Backend/Domain/Entities/.
- Tầng Nghiệp vụ (Features): Mã nguồn xử lý logic được phân chia theo từng chức năng cụ thể (Use Cases). Khi phát triển một tính năng mới, lập trình viên tạo tệp Endpoint (hoặc Handler) trực tiếp tại thư mục của phân hệ đó (Ví dụ: src/Org.Backend/Features/Tasks/). Khuyến cáo không khởi tạo các lớp Controller hay Service phân tán bên ngoài định dạng phân hệ chức năng.
- Tầng Giao tiếp (Shared Contracts): Các đối tượng truyền tải dữ liệu (Data Transfer Objects - DTOs) dùng để giao tiếp giữa Frontend và Backend phải được định nghĩa tại src/Org.Shared/Features/ nhằm đảm bảo tính đồng bộ và an toàn kiểu dữ liệu.

## 5. Quản lý tài nguyên và dọn dẹp hệ thống

Để giải phóng tài nguyên phần cứng (RAM/CPU) sau quá trình làm việc, hệ thống cơ sở dữ liệu ảo hóa cần được ngưng đọng bằng lệnh sau tại thư mục gốc:

`docker compose down`

*Ghi chú: Thao tác này chỉ tạm dừng và gỡ bỏ container, toàn bộ dữ liệu thực tế của cơ sở dữ liệu vẫn được lưu trữ an toàn trong phân vùng ảo (Volume) của Docker và sẽ được phục hồi nguyên vẹn trong lần khởi chạy tiếp theo.*
