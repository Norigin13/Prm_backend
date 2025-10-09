# 📁 Cấu Trúc Service MyBackend

## 🎯 Tổng Quan
Dự án MyBackend đã được tái cấu trúc để tách các service riêng biệt, giúp code dễ đọc, dễ bảo trì và mở rộng hơn.

## 📂 Cấu Trúc Thư Mục

```
MyBackend/
├── Data/
│   └── AppDbContext.cs          # Database Context & Models
├── Services/
│   ├── UserService.cs           # Quản lý User & Authentication
│   ├── ProductService.cs        # Quản lý Sản phẩm
│   ├── CategoryService.cs       # Quản lý Danh mục
│   ├── SupplierService.cs       # Quản lý Nhà cung cấp
│   ├── ServiceEntityService.cs  # Quản lý Dịch vụ
│   ├── ServiceOrderService.cs   # Quản lý Đơn hàng dịch vụ
│   └── ServiceFeedbackService.cs # Quản lý Đánh giá dịch vụ
├── Program.cs                   # Main application & endpoints
└── ...
```

## 🔧 Các Service

### 1. **UserService** 👤
- **Chức năng**: Quản lý người dùng và xác thực
- **Endpoints**:
  - `POST /api/user/register` - Đăng ký
  - `POST /api/user/login` - Đăng nhập
  - `GET /api/user/{id}` - Lấy thông tin user
  - `PUT /api/user/{id}` - Cập nhật user
  - `DELETE /api/user/{id}` - Xóa user
  - `GET /api/users` - Lấy danh sách users

### 2. **ProductService** 📦
- **Chức năng**: Quản lý sản phẩm
- **Endpoints**:
  - `GET /api/product` - Lấy danh sách sản phẩm
  - `GET /api/product/{id}` - Lấy chi tiết sản phẩm
  - `POST /api/product` - Tạo sản phẩm mới
  - `PUT /api/product/{id}` - Cập nhật sản phẩm
  - `DELETE /api/product/{id}` - Xóa sản phẩm

### 3. **CategoryService** 📂
- **Chức năng**: Quản lý danh mục sản phẩm
- **Endpoints**:
  - `GET /api/category` - Lấy danh sách danh mục
  - `GET /api/category/{id}` - Lấy chi tiết danh mục
  - `POST /api/category` - Tạo danh mục mới
  - `PUT /api/category/{id}` - Cập nhật danh mục
  - `DELETE /api/category/{id}` - Xóa danh mục
  - `GET /api/category/{id}/products` - Lấy sản phẩm theo danh mục

### 4. **SupplierService** 🏢
- **Chức năng**: Quản lý nhà cung cấp
- **Endpoints**:
  - `GET /api/supplier` - Lấy danh sách nhà cung cấp
  - `GET /api/supplier/{id}` - Lấy chi tiết nhà cung cấp
  - `POST /api/supplier` - Tạo nhà cung cấp mới
  - `PUT /api/supplier/{id}` - Cập nhật nhà cung cấp
  - `DELETE /api/supplier/{id}` - Xóa nhà cung cấp
  - `GET /api/supplier/{id}/products` - Lấy sản phẩm theo nhà cung cấp

### 5. **ServiceEntityService** 🛠️
- **Chức năng**: Quản lý các dịch vụ
- **Endpoints**:
  - `GET /api/service` - Lấy danh sách dịch vụ
  - `GET /api/service/{id}` - Lấy chi tiết dịch vụ
  - `POST /api/service` - Tạo dịch vụ mới
  - `PUT /api/service/{id}` - Cập nhật dịch vụ
  - `DELETE /api/service/{id}` - Xóa dịch vụ

### 6. **ServiceOrderService** 📋
- **Chức năng**: Quản lý đơn hàng dịch vụ
- **Endpoints**:
  - `GET /api/service-order` - Lấy danh sách đơn hàng
  - `GET /api/service-order/{id}` - Lấy chi tiết đơn hàng
  - `POST /api/service-order` - Tạo đơn hàng mới
  - `PUT /api/service-order/{id}` - Cập nhật đơn hàng
  - `DELETE /api/service-order/{id}` - Xóa đơn hàng
  - `GET /api/service-order/user/{userId}` - Lấy đơn hàng theo user

### 7. **ServiceFeedbackService** ⭐
- **Chức năng**: Quản lý đánh giá dịch vụ
- **Endpoints**:
  - `GET /api/service-feedback` - Lấy danh sách đánh giá
  - `GET /api/service-feedback/{id}` - Lấy chi tiết đánh giá
  - `POST /api/service-feedback` - Tạo đánh giá mới
  - `PUT /api/service-feedback/{id}` - Cập nhật đánh giá
  - `DELETE /api/service-feedback/{id}` - Xóa đánh giá
  - `GET /api/service-feedback/service-order/{id}` - Lấy đánh giá theo đơn hàng
  - `GET /api/service-feedback/user/{userId}` - Lấy đánh giá theo user

## 🎨 Lợi Ích Của Cấu Trúc Mới

### ✅ **Dễ Đọc**
- Mỗi service chỉ xử lý một domain cụ thể
- Code được tổ chức rõ ràng theo chức năng
- Dễ dàng tìm kiếm và hiểu logic

### ✅ **Dễ Bảo Trì**
- Thay đổi logic chỉ ảnh hưởng đến service tương ứng
- Dễ dàng debug và fix lỗi
- Có thể test từng service độc lập

### ✅ **Dễ Mở Rộng**
- Thêm tính năng mới không ảnh hưởng code cũ
- Có thể thêm validation, caching riêng cho từng service
- Dễ dàng tích hợp với các middleware khác

### ✅ **Tái Sử Dụng**
- Các service có thể được sử dụng ở nhiều nơi
- Logic business được tách biệt khỏi endpoint
- Dễ dàng tạo unit tests

## 🚀 Cách Chạy

```bash
cd D:\exe\.Net_Ezbuild\MyBackend
dotnet run
```

## 📍 API Base URL
- **HTTP**: `http://localhost:5162`
- **HTTPS**: `https://localhost:7229`
- **Swagger UI**: `http://localhost:5162/swagger`

## 🔧 Database
- **MySQL**: `mysql-ezbuildvndb.alwaysdata.net`
- **SQLite**: `ezbuild.db` (fallback)

---

*Cấu trúc này giúp code dễ đọc, dễ bảo trì và mở rộng hơn rất nhiều so với việc viết tất cả trong một file Program.cs duy nhất!* 🎉
