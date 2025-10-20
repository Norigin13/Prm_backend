# 📁 Cấu Trúc Service PRM_Backend

## 🎯 Tổng Quan

Dự án PRM_Backend đã được tái cấu trúc để tách các service riêng biệt, giúp code dễ đọc, dễ bảo trì và mở rộng hơn.

## 📂 Cấu Trúc Thư Mục

```
PRM_Backend/
├── Data/
│   └── AppDbContext.cs          # Database Context & Models
├── Services/
│   ├── UserService.cs           # Quản lý User & Authentication
│   ├── ProductService.cs        # Quản lý Sản phẩm
│   ├── CategoryService.cs       # Quản lý Danh mục
│   ├── SupplierService.cs       # Quản lý Nhà cung cấp
│   ├── ServiceEntityService.cs  # Quản lý Dịch vụ
│   ├── ServiceOrderService.cs   # Quản lý Đơn hàng dịch vụ
│   ├── ServiceFeedbackService.cs # Quản lý Đánh giá dịch vụ
│   ├── ChatService.cs           # Quản lý Chat AI với Google Gemini
│   ├── BuildService.cs          # Quản lý Build PC
│   └── OrderService.cs          # Quản lý Đơn hàng
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

### 8. **ChatService** 🤖

- **Chức năng**: Quản lý Chat AI với Google Gemini
- **Endpoints**:
  - `POST /api/chat/send` - Gửi tin nhắn chat với AI
  - `POST /api/chat/simple` - Chat đơn giản (chỉ cần message)
  - `GET /api/chat/history` - Lấy lịch sử chat

### 9. **BuildService** 🔧

- **Chức năng**: Quản lý Build PC
- **Endpoints**:
  - `GET /api/build` - Lấy danh sách build
  - `GET /api/build/{id}` - Lấy chi tiết build
  - `POST /api/build` - Tạo build mới
  - `PUT /api/build/{id}` - Cập nhật build
  - `DELETE /api/build/{id}` - Xóa build
  - `GET /api/build/user/{userId}` - Lấy build theo user

### 10. **OrderService** 📦

- **Chức năng**: Quản lý đơn hàng
- **Endpoints**:
  - `GET /api/order` - Lấy danh sách đơn hàng
  - `GET /api/order/{id}` - Lấy chi tiết đơn hàng
  - `GET /api/order/user/{userId}` - Lấy đơn hàng theo user
  - `POST /api/order` - Tạo đơn hàng mới
  - `PUT /api/order/{id}` - Cập nhật đơn hàng
  - `DELETE /api/order/{id}` - Xóa đơn hàng

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
cd D:\exe\.Net_Ezbuild\PRM_Backend
dotnet run
```

## 📍 API Base URL

- **HTTP**: `http://localhost:5162`
- **HTTPS**: `https://localhost:7229`
- **Swagger UI**: `http://localhost:5162/swagger`

## 🔧 Database

- **MySQL**: `mysql-ezbuildvndb.alwaysdata.net`
- **SQLite**: `ezbuild.db` (fallback)

## 🤖 AI Integration

- **Google Gemini API**: Tích hợp Chat AI với Google Gemini 2.5 Flash
- **Cấu hình**: Thêm `GoogleAI:ApiKey` vào `appsettings.json`
- **Tính năng**: Tư vấn PC, build configuration, hỗ trợ tiếng Việt

## 📊 Entity Mới

- **Build**: Quản lý cấu hình PC
- **BuildItem**: Chi tiết linh kiện trong build
- **Order**: Đơn hàng mua build
- **ChatMessage**: Lịch sử chat với AI

---

_Cấu trúc này giúp code dễ đọc, dễ bảo trì và mở rộng hơn rất nhiều so với việc viết tất cả trong một file Program.cs duy nhất!_ 🎉

## 🆕 Cập Nhật Mới

- ✅ Thêm Chat AI với Google Gemini
- ✅ Thêm Build Management System
- ✅ Thêm Order Management System
- ✅ Tích hợp đầy đủ với Spring Boot backend
