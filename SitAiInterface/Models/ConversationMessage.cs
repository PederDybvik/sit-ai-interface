using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SitAiInterface.Models;

public class ConversationMessage
{
    /// <summary>Who sent this message. Must be "user" or "assistant".</summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageRole Role { get; set; }

    /// <summary>The text content of the message.</summary>
    [Required]
    [MinLength(1)]
    public string Content { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    User,
    Assistant
}
