using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum ChatSender
    {
        User,
        Bot
    }

    // AI Description Generation module — draft copy, gated behind staff approval
    public class AIGeneratedDescription
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public string GeneratedText { get; set; } = string.Empty;

        [MaxLength(30)]
        public string ModelVersion { get; set; } = "claude-sonnet-5";

        // Never shown publicly until a staff member approves it
        public bool IsApproved { get; set; } = false;

        public int? ReviewedById { get; set; }

        [ForeignKey(nameof(ReviewedById))]
        public User? ReviewedBy { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // AI Chatbot module — adopter FAQ support
    public class ChatbotConversation
    {
        public int Id { get; set; }

        // Null for anonymous site visitors
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required, MaxLength(64)]
        public string SessionId { get; set; } = string.Empty;

        public bool HandedOffToStaff { get; set; } = false;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatbotMessage>? Messages { get; set; }
    }

    public class ChatbotMessage
    {
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public ChatbotConversation? Conversation { get; set; }

        [Required]
        public ChatSender Sender { get; set; }

        [Required]
        public string MessageText { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
