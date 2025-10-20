# PRM_Backend

## 🎯 Tổng Quan

PRM_Backend là một RESTful API backend được xây dựng bằng .NET 9.0, cung cấp các tính năng quản lý sản phẩm, người dùng, đơn hàng và tích hợp AI chat với Google Gemini.

## 🚀 Tính Năng Chính

### 👤 Quản Lý Người Dùng
- Đăng ký/Đăng nhập
- Quản lý thông tin cá nhân
- Phân quyền người dùng

### 📦 Quản Lý Sản Phẩm
- CRUD operations cho sản phẩm
- Quản lý danh mục và nhà cung cấp
- Quản lý giá sản phẩm

### 🔧 Build PC Management
- Tạo và quản lý cấu hình PC
- Quản lý linh kiện trong build
- Tính toán tổng giá build

### 📋 Quản Lý Đơn Hàng
- Tạo đơn hàng từ build
- Theo dõi trạng thái đơn hàng
- Quản lý thông tin thanh toán

### 🤖 AI Chat Integration
- Tích hợp Google Gemini AI
- Tư vấn PC chuyên nghiệp
- Hỗ trợ tiếng Việt

### 🛠️ Dịch Vụ
- Quản lý dịch vụ
- Đặt lịch dịch vụ
- Đánh giá dịch vụ

## 🛠️ Công Nghệ Sử Dụng

- **.NET 9.0** - Framework chính
- **Entity Framework Core** - ORM
- **SQLite/MySQL** - Database
- **Swagger/OpenAPI** - API Documentation
- **Google Gemini AI** - AI Integration
- **BCrypt** - Password Hashing

## 📋 Yêu Cầu Hệ Thống

- .NET 9.0 SDK
- SQLite (mặc định) hoặc MySQL
- Google AI API Key (cho tính năng chat)

## 🚀 Cài Đặt và Chạy

### 1. Clone Repository
```bash
git clone <repository-url>
cd PRM_Backend
```

### 2. Cài Đặt Dependencies
```bash
dotnet restore
```

### 3. Cấu Hình Database
Cập nhật `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MySql": "your-mysql-connection-string",
    "Sqlite": "Data Source=ezbuild.db"
  }
}
```

### 4. Cấu Hình Google AI (Tùy chọn)
```json
{
  "GoogleAI": {
    "ApiKey": "your-google-ai-api-key"
  }
}
```

### 5. Chạy Ứng Dụng
```bash
dotnet run
```

### 6. Truy Cập API
- **API Base URL**: `http://localhost:5162`
- **Swagger UI**: `http://localhost:5162/swagger`

## 📚 API Documentation

### Endpoints Chính

#### User Management
- `POST /api/user/register` - Đăng ký
- `POST /api/user/login` - Đăng nhập
- `GET /api/user/{id}` - Lấy thông tin user
- `PUT /api/user/{id}` - Cập nhật user
- `DELETE /api/user/{id}` - Xóa user

#### Product Management
- `GET /api/product` - Lấy danh sách sản phẩm
- `POST /api/product` - Tạo sản phẩm mới
- `PUT /api/product/{id}` - Cập nhật sản phẩm
- `DELETE /api/product/{id}` - Xóa sản phẩm

#### Build Management
- `GET /api/build` - Lấy danh sách build
- `POST /api/build` - Tạo build mới
- `GET /api/build/{id}` - Lấy chi tiết build
- `PUT /api/build/{id}` - Cập nhật build
- `DELETE /api/build/{id}` - Xóa build

#### Order Management
- `GET /api/order` - Lấy danh sách đơn hàng
- `POST /api/order` - Tạo đơn hàng mới
- `GET /api/order/{id}` - Lấy chi tiết đơn hàng
- `PUT /api/order/{id}` - Cập nhật đơn hàng

#### AI Chat
- `POST /api/chat/send` - Gửi tin nhắn chat
- `POST /api/chat/simple` - Chat đơn giản
- `GET /api/chat/history` - Lấy lịch sử chat

## 🗄️ Database Schema

### Tables Chính
- `users` - Thông tin người dùng
- `products` - Sản phẩm
- `categories` - Danh mục sản phẩm
- `suppliers` - Nhà cung cấp
- `product_prices` - Giá sản phẩm
- `builds` - Cấu hình PC
- `build_items` - Linh kiện trong build
- `orders` - Đơn hàng
- `chat_messages` - Lịch sử chat AI
- `services` - Dịch vụ
- `service_orders` - Đơn hàng dịch vụ
- `service_feedbacks` - Đánh giá dịch vụ

## 🔧 Cấu Hình

### Environment Variables
```bash
# Database
ConnectionStrings__MySql="your-mysql-connection"
ConnectionStrings__Sqlite="Data Source=ezbuild.db"

# Google AI
GoogleAI__ApiKey="your-api-key"
```

### User Secrets (Development)
```bash
dotnet user-secrets set "GoogleAI:ApiKey" "your-api-key"
```

## 🧪 Testing

### Chạy Tests
```bash
dotnet test
```

### API Testing với Swagger
1. Truy cập `http://localhost:5162/swagger`
2. Sử dụng "Try it out" để test các endpoints
3. Xem request/response examples

## 📈 Performance

### Optimization Tips
- Sử dụng async/await cho tất cả database operations
- Implement caching cho AI responses
- Sử dụng pagination cho large datasets
- Monitor database query performance

## 🔒 Security

### Best Practices
- Sử dụng HTTPS trong production
- Hash passwords với BCrypt
- Validate input data
- Implement rate limiting
- Secure API keys

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

## 📄 License

MIT License

## 📞 Support

Nếu có vấn đề hoặc câu hỏi, vui lòng tạo issue trên GitHub repository.

---

*PRM_Backend - Professional PC Management Backend* 🎉
