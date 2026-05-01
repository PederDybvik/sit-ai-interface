using Microsoft.AspNetCore.Mvc;
using SitAiInterface.Models;
using SitAiInterface.Services;

namespace SitAiInterface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>Send a message (with optional conversation history) and get an AI response.</summary>
    /// <remarks>
    /// Pass the full conversation so far in `messages`. For a fresh conversation, send a single entry with Role=User.
    /// For follow-up questions, include prior User/Assistant turns before the latest User message.
    /// The last message must have Role=User.
    ///
    /// Example — first question:
    ///
    ///     POST /api/chat
    ///     { "messages": [{ "role": "User", "content": "Hvor finner jeg studentboliger?" }] }
    ///
    /// Example — follow-up:
    ///
    ///     POST /api/chat
    ///     {
    ///       "messages": [
    ///         { "role": "User",      "content": "Hvor finner jeg studentboliger?" },
    ///         { "role": "Assistant", "content": "SIT tilbyr boliger på Berg og Nedre Berg..." },
    ///         { "role": "User",      "content": "Hva koster det?" }
    ///       ]
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var last = request.Messages.LastOrDefault();
        if (last is null || last.Role != Models.MessageRole.User)
            return BadRequest(new { error = "The last message must have Role = User." });

        _logger.LogInformation("Chat request received ({Turns} turn(s)): {Message}",
            request.Messages.Count, last.Content);

        var response = await _chatService.GetResponseAsync(request.Messages);
        return Ok(response);
    }
}
