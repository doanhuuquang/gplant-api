using Gplant.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Gplant.API.Controllers
{
    public record ChatbotRequest(string Question);

    [Route("api/chatbot")]
    [ApiController]
    public class ChatBotController(AIChatService aiChatService) : ControllerBase
    {
        [HttpPost("ask/stream")]
        public async Task AskStream([FromBody] ChatbotRequest request)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = Guid.TryParse(userIdStr, out Guid userId);

            await foreach (var chunk in aiChatService.ChatStreamAsync(request.Question, userId))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}