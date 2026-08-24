using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Chatbot;
using PawTrack.Api.Services;
using System.Security.Claims;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        // POST api/chatbot/message
        [HttpPost("message")]
        public async Task<ActionResult<ChatbotConversationDto>> SendMessage([FromBody] SendChatMessageDto dto)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return Ok(await _chatbotService.SendMessageAsync(userId, dto));
        }

        // GET api/chatbot/session/abc123
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<ChatbotConversationDto>> GetSession(string sessionId)
        {
            var conversation = await _chatbotService.GetBySessionIdAsync(sessionId);
            if (conversation is null) return NotFound();
            return Ok(conversation);
        }
    }
}
