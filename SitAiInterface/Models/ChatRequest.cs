using System.ComponentModel.DataAnnotations;

namespace SitAiInterface.Models;

public class ChatRequest
{
    /// <summary>
    /// The full conversation history. The last entry must have Role = User.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<ConversationMessage> Messages { get; set; } = [];
}
