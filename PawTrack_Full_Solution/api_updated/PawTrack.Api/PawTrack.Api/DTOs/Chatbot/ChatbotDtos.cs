using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Chatbot
{
    public class ChatbotMessageDto
    {
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ChatbotConversationDto
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public bool HandedOffToStaff { get; set; }
        public DateTime StartedAt { get; set; }
        public List<ChatbotMessageDto> Messages { get; set; } = new();
    }

    public class SendChatMessageDto
    {
        // Client generates/keeps a SessionId per browser session; works anonymously
        [Required, MaxLength(64)]
        public string SessionId { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string MessageText { get; set; } = string.Empty;
    }
}
