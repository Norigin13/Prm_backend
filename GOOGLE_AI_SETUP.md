# 🤖 Hướng Dẫn Cấu Hình Google AI API

## 📋 Yêu Cầu

- Google AI API Key từ Google AI Studio
- .NET 9.0 hoặc cao hơn

## 🔑 Lấy API Key

### Bước 1: Truy cập Google AI Studio

1. Đi tới [Google AI Studio](https://aistudio.google.com/)
2. Đăng nhập bằng tài khoản Google
3. Tạo API key mới

### Bước 2: Cấu hình API Key

1. Mở file `appsettings.json`
2. Thêm API key vào cấu hình:

```json
{
  "GoogleAI": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### Bước 3: Cấu hình cho Production

Để bảo mật, sử dụng User Secrets hoặc Environment Variables:

```bash
# User Secrets (Development)
dotnet user-secrets set "GoogleAI:ApiKey" "YOUR_API_KEY_HERE"

# Environment Variables (Production)
export GoogleAI__ApiKey="YOUR_API_KEY_HERE"
```

## 🚀 Sử Dụng API

### Chat Endpoint

```bash
POST /api/chat/send
Content-Type: application/json

{
  "message": "Tôi muốn build PC gaming với budget 20 triệu",
  "userId": "user123",
  "chatHistory": []
}
```

### Simple Chat Endpoint

```bash
POST /api/chat/simple
Content-Type: application/json

{
  "message": "CPU nào tốt nhất cho gaming?"
}
```

### Lấy Lịch Sử Chat

```bash
GET /api/chat/history?userId=123
```

## 🔧 Tính Năng AI

### Chuyên Gia Tư Vấn PC

- Tư vấn cấu hình PC gaming
- Tư vấn workstation
- Budget build recommendations
- Compatibility checking
- Performance benchmarks

### Hỗ Trợ Tiếng Việt

- Trả lời bằng tiếng Việt
- Hiểu ngữ cảnh Việt Nam
- Giá cả và thị trường Việt Nam

## 🛠️ Troubleshooting

### Lỗi API Key

```
API key chưa được cấu hình. Vui lòng thêm GoogleAI:ApiKey vào appsettings.json
```

**Giải pháp**: Kiểm tra cấu hình API key trong `appsettings.json`

### Lỗi Rate Limit

```
Lỗi khi gọi Google AI API: TooManyRequests
```

**Giải pháp**: Đợi một lúc rồi thử lại, hoặc nâng cấp quota

### Lỗi Network

```
Có lỗi xảy ra khi xử lý yêu cầu: HttpRequestException
```

**Giải pháp**: Kiểm tra kết nối internet và firewall

## 📊 Monitoring

### Logs

ChatService sẽ log các thông tin:

- Thành công: `Chat request processed successfully for user: {UserId}`
- Lỗi: `Error processing chat request: {Exception}`

### Database

Tất cả chat messages được lưu vào bảng `chat_messages`:

- `user_id`: ID người dùng
- `message`: Tin nhắn gửi đi
- `response`: Phản hồi từ AI
- `created_at`: Thời gian tạo

## 🔒 Bảo Mật

### API Key Security

- Không commit API key vào Git
- Sử dụng User Secrets cho development
- Sử dụng Environment Variables cho production
- Rotate API key định kỳ

### Rate Limiting

- Google AI có giới hạn requests per minute
- Implement caching cho responses thường dùng
- Monitor usage để tránh vượt quota

## 📈 Performance

### Optimization Tips

- Cache responses cho câu hỏi thường gặp
- Sử dụng async/await cho tất cả API calls
- Implement timeout cho HTTP requests
- Monitor response times

### Caching Strategy

```csharp
// Example caching implementation
private readonly IMemoryCache _cache;

public async Task<ChatResponse> ChatAsync(ChatRequest request)
{
    var cacheKey = $"chat_{request.Message.GetHashCode()}";
    if (_cache.TryGetValue(cacheKey, out ChatResponse cachedResponse))
    {
        return cachedResponse;
    }

    // Call AI API and cache result
    var response = await CallAIApi(request);
    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
    return response;
}
```

---

_Cấu hình này giúp tích hợp Google AI một cách an toàn và hiệu quả vào .NET backend!_ 🎉
