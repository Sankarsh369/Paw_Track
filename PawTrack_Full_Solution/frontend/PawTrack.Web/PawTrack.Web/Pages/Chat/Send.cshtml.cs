using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Chat
{
    [IgnoreAntiforgeryToken]
    public class SendModel : PageModel
    {
        private readonly ApiClient _api;

        public SendModel(ApiClient api)
        {
            _api = api;
        }

        public class SendChatRequest
        {
            public string SessionId { get; set; } = string.Empty;
            public string MessageText { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync([FromBody] SendChatRequest request)
        {
            var (success, data, error) = await _api.PostAsync<ChatConversationModel>(
                "api/chatbot/message",
                new { sessionId = request.SessionId, messageText = request.MessageText });

            if (!success || data is null)
                return new JsonResult(new { error = error ?? "Could not reach the assistant." }) { StatusCode = 502 };

            return new JsonResult(data);
        }
    }
}
