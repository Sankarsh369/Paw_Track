using PawTrack.Api.DTOs.Chatbot;

namespace PawTrack.Api.Services
{
    public interface IChatbotService
    {
        Task<ChatbotConversationDto> SendMessageAsync(int? userId, SendChatMessageDto dto);
        Task<ChatbotConversationDto?> GetBySessionIdAsync(string sessionId);
    }
}
