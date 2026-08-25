using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Chatbot;
using PawTrack.Api.Models;
using System.Text;
using System.Text.Json;

namespace PawTrack.Api.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly PawTrackDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatbotService(PawTrackDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public ChatbotService(PawTrackDbContext context)
            : this(context, new HttpClient(), new ConfigurationBuilder().AddEnvironmentVariables().Build())
        {
        }

        public async Task<ChatbotConversationDto> SendMessageAsync(int? userId, SendChatMessageDto dto)
        {
            var conversation = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SessionId == dto.SessionId);

            if (conversation is null)
            {
                conversation = new ChatbotConversation { SessionId = dto.SessionId, UserId = userId };
                _context.ChatbotConversations.Add(conversation);
            }

            var userMessage = new ChatbotMessage
            {
                Conversation = conversation,
                Sender = ChatSender.User,
                MessageText = dto.MessageText
            };
            _context.ChatbotMessages.Add(userMessage);

            var history = conversation.Messages?.ToList();
            var (reply, handoff) = await BuildReplyAsync(dto.MessageText, history);
            if (handoff) conversation.HandedOffToStaff = true;

            var botMessage = new ChatbotMessage
            {
                Conversation = conversation,
                Sender = ChatSender.Bot,
                MessageText = reply
            };
            _context.ChatbotMessages.Add(botMessage);

            await _context.SaveChangesAsync();

            var full = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == conversation.Id);

            return ToDto(full);
        }

        public async Task<ChatbotConversationDto?> GetBySessionIdAsync(string sessionId)
        {
            var conversation = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            return conversation is null ? null : ToDto(conversation);
        }

        private async Task<(string reply, bool handoff)> BuildReplyAsync(string messageText, List<ChatbotMessage>? history)
        {
            var textLower = messageText.ToLowerInvariant();
            bool handoffRequested = textLower.Contains("staff") || 
                                    textLower.Contains("human") || 
                                    textLower.Contains("person") || 
                                    textLower.Contains("support") || 
                                    textLower.Contains("contact");

            var dbContextPrompt = await BuildDatabaseContextPromptAsync();
            var geminiResponse = await CallGeminiChatApiAsync(messageText, history, dbContextPrompt);

            if (handoffRequested)
            {
                var combinedReply = $"{geminiResponse}\n\n*(Note: I have also flagged this conversation for our shelter staff. A team member will review our chat history if you need further assistance!)*";
                return (combinedReply, true);
            }

            return (geminiResponse, false);
        }

        private async Task<string> BuildDatabaseContextPromptAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are PawTrack AI, an intelligent, friendly, and helpful virtual assistant for the PawTrack animal shelter network.");
            sb.AppendLine("Use the following live PawTrack database context to provide clear, accurate, and helpful natural-language answers to adopters and visitors:");

            try
            {
                var animals = await _context.Animals
                    .Include(a => a.Branch)
                    .Include(a => a.Category)
                    .Where(a => a.Status == AnimalStatus.Available)
                    .Take(15)
                    .ToListAsync();

                sb.AppendLine("\n--- CURRENT AVAILABLE ANIMALS FOR ADOPTION ---");
                if (animals.Any())
                {
                    foreach (var a in animals)
                    {
                        sb.AppendLine($"- ID: {a.Id}, Name: {a.Name}, Species: {a.Species}, Breed: {a.Breed}, Age: {a.Age} yrs, Gender: {a.Gender}, Branch: {a.Branch?.Name ?? "Main Shelter"}");
                    }
                }
                else
                {
                    sb.AppendLine("No animals are currently listed as available.");
                }
            }
            catch
            {
                sb.AppendLine("Animal context unavailable.");
            }

            try
            {
                var branches = await _context.Branches.ToListAsync();
                sb.AppendLine("\n--- SHELTER BRANCH LOCATIONS ---");
                if (branches.Any())
                {
                    foreach (var b in branches)
                    {
                        sb.AppendLine($"- Name: {b.Name}, Address: {b.Address}, City/Region: {b.RegionCity}, Phone: {b.Phone ?? "N/A"}");
                    }
                }
            }
            catch
            {
                sb.AppendLine("Branch context unavailable.");
            }

            try
            {
                var slots = await _context.VisitSlots
                    .Where(s => s.SlotDate >= DateTime.UtcNow.Date && s.BookedCount < s.Capacity)
                    .OrderBy(s => s.SlotDate)
                    .Take(15)
                    .ToListAsync();

                sb.AppendLine("\n--- UPCOMING OPEN VISIT SLOTS ---");
                if (slots.Any())
                {
                    foreach (var s in slots)
                    {
                        sb.AppendLine($"- Branch ID: {s.BranchId}, Date: {s.SlotDate:yyyy-MM-dd}, Time: {s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}, Open Spots: {s.Capacity - s.BookedCount}");
                    }
                }
            }
            catch
            {
                sb.AppendLine("Visit slots context unavailable.");
            }

            sb.AppendLine("\nGuidelines:");
            sb.AppendLine("- Provide warm, accurate, concise, and helpful responses based on the database context provided above.");
            sb.AppendLine("- When referencing animals, mention their name, breed, age, and suggest viewing their profile: [View Profile](/Animals/Details/{Id}).");
            sb.AppendLine("- If the user asks general adoption questions, explain PawTrack adoption procedures (browse animals -> book visit slot -> submit application).");
            sb.AppendLine("- Maintain a friendly, supportive tone at all times.");

            return sb.ToString();
        }

        private async Task<string> CallGeminiChatApiAsync(string userMessage, List<ChatbotMessage>? history, string dbContextPrompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"]
                         ?? _configuration["GEMINI_API_KEY"]
                         ?? _configuration["GeminiApiKey"]
                         ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Please set 'Gemini:ApiKey' in configuration or 'GEMINI_API_KEY' environment variable.");
            }

            var contentsList = new List<object>();

            if (history != null && history.Any())
            {
                foreach (var msg in history.TakeLast(10))
                {
                    var role = msg.Sender == ChatSender.User ? "user" : "model";
                    contentsList.Add(new
                    {
                        role = role,
                        parts = new[] { new { text = msg.MessageText } }
                    });
                }
            }

            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var requestPayload = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = dbContextPrompt }
                    }
                },
                contents = contentsList
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsync(requestUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API request failed with status code {response.StatusCode}: {errorResponse}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textProp))
            {
                return textProp.GetString()?.Trim() ?? string.Empty;
            }

            throw new InvalidOperationException("Failed to parse a valid response text from Gemini API.");
        }

        private static ChatbotConversationDto ToDto(ChatbotConversation c) => new()
        {
            Id = c.Id,
            SessionId = c.SessionId,
            HandedOffToStaff = c.HandedOffToStaff,
            StartedAt = c.StartedAt,
            Messages = (c.Messages ?? new List<ChatbotMessage>())
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatbotMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender.ToString(),
                    MessageText = m.MessageText,
                    Timestamp = m.Timestamp
                }).ToList()
        };
    }
}
