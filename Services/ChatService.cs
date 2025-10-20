using PRM_Backend.Data;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PRM_Backend.Services;

public class ChatService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AppDbContext context, HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        try
        {
            var apiKey = _configuration["GoogleAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = "API key chưa được cấu hình. Vui lòng thêm GoogleAI:ApiKey vào appsettings.json",
                    Timestamp = DateTime.UtcNow
                };
            }

            // Xây dựng prompt
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Bạn là một chuyên gia tư vấn PC và linh kiện máy tính với hơn 10 năm kinh nghiệm. ");
            promptBuilder.AppendLine("Bạn có kiến thức sâu rộng về: CPU, GPU, RAM, SSD, HDD, PSU, Mainboard, Case, Cooling, Monitor, Keyboard, Mouse. ");
            promptBuilder.AppendLine("Bạn hiểu rõ về: Gaming PC, Workstation, Budget build, High-end build, Overclocking, Compatibility, Performance benchmarks. ");
            promptBuilder.AppendLine("Hãy trả lời bằng tiếng Việt một cách chuyên nghiệp và hữu ích. ");
            promptBuilder.AppendLine("YÊU CẦU ĐẦU RA: mở đầu bằng 1 câu chào ngắn gọn thân thiện; sau đó trả lời ngắn gọn, đúng trọng tâm, tối đa 6 gạch đầu dòng. ");
            promptBuilder.AppendLine("NẾU câu hỏi không liên quan tới PC hoặc thông tin/độ dài vượt giới hạn nên không thể trả lời đầy đủ, hãy trả lời đúng nguyên văn: 'Nội dung này không thể trả lời cho user'. ");
            promptBuilder.AppendLine("Nếu câu hỏi là build PC, xuất cấu hình đề xuất dạng bullets gồm: CPU, Mainboard, RAM, GPU, SSD, PSU, Case/Cooling, Giá ước tính.");
            promptBuilder.AppendLine();

            // Thêm lịch sử chat nếu có
            if (request.ChatHistory != null && request.ChatHistory.Any())
            {
                promptBuilder.AppendLine("Lịch sử cuộc trò chuyện:");
                foreach (var msg in request.ChatHistory)
                {
                    promptBuilder.AppendLine($"{msg.Role}: {msg.Content}");
                }
                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine($"Người dùng: {request.Message}");

            // Tạo request body theo format REST v1
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptBuilder.ToString() }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            var url = "https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var aiResponse = ExtractTextFromResponse(responseContent);

                if (!string.IsNullOrEmpty(aiResponse))
                {
                    // Không lưu chat message vào database để tránh lỗi
                    // var chatMessage = new ChatMessage
                    // {
                    //     UserId = int.TryParse(request.UserId, out var userId) ? userId : null,
                    //     Message = request.Message,
                    //     Response = aiResponse,
                    //     CreatedAt = DateTime.UtcNow
                    // };

                    // _context.ChatMessages.Add(chatMessage);
                    // await _context.SaveChangesAsync();

                    _logger.LogInformation("Chat request processed successfully for user: {UserId}", request.UserId);
                    return new ChatResponse
                    {
                        Message = request.Message,
                        Response = aiResponse,
                        UserId = request.UserId,
                        Timestamp = DateTime.UtcNow,
                        Success = true
                    };
                }

                return new ChatResponse
                {
                    Success = false,
                    Error = $"Phản hồi không có nội dung từ AI. Body: {responseContent}",
                    Timestamp = DateTime.UtcNow
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ChatResponse
            {
                Success = false,
                Error = $"Lỗi khi gọi Google AI API: {response.StatusCode}, Body: {errorContent}",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return new ChatResponse
            {
                Success = false,
                Error = $"Có lỗi xảy ra khi xử lý yêu cầu: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private string? ExtractTextFromResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content))
                {
                    if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString();
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from AI response");
            return null;
        }
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync(int? userId = null)
    {
        // Trả về danh sách rỗng để tránh lỗi database
        return await Task.FromResult(new List<ChatMessage>());
        
        // Code cũ bị comment để tránh lỗi database
        // var query = _context.ChatMessages.AsQueryable();
        // 
        // if (userId.HasValue)
        // {
        //     query = query.Where(c => c.UserId == userId);
        // }
        //
        // return await query
        //     .OrderByDescending(c => c.CreatedAt)
        //     .Take(50)
        //     .ToListAsync();
    }
}

// DTOs
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<ChatMessageDto>? ChatHistory { get; set; }
}

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class SimpleChatRequest
{
    public string Message { get; set; } = string.Empty;
}
