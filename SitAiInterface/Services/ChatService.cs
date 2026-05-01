using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using SitAiInterface.Models;

namespace SitAiInterface.Services;

public class ChatService
{
    private readonly ChatClient _chatClient;
    private readonly KnowledgeBaseService _knowledgeBase;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IConfiguration configuration,
        KnowledgeBaseService knowledgeBase,
        ILogger<ChatService> logger)
    {
        _knowledgeBase = knowledgeBase;
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var openAiClient = new OpenAIClient(apiKey);
        _chatClient = openAiClient.GetChatClient(model);
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<SitAiInterface.Models.ConversationMessage> history)
    {
        var systemPrompt = _knowledgeBase.BuildSystemPrompt();

        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

        foreach (var msg in history)
        {
            messages.Add(msg.Role == SitAiInterface.Models.MessageRole.Assistant
                ? new AssistantChatMessage(msg.Content)
                : new UserChatMessage(msg.Content));
        }

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            Temperature = 0.3f,
            MaxOutputTokenCount = 512
        };

        try
        {
            var completion = await _chatClient.CompleteChatAsync(messages, options);
            var raw = completion.Value.Content[0].Text;

            _logger.LogDebug("OpenAI raw response: {Raw}", raw);

            return ParseResponse(raw);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }

    private static ChatResponse ParseResponse(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var answer = root.TryGetProperty("answer", out var answerProp)
                ? answerProp.GetString() ?? string.Empty
                : string.Empty;

            var sources = new List<string>();
            if (root.TryGetProperty("sources", out var sourcesProp) &&
                sourcesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in sourcesProp.EnumerateArray())
                {
                    var url = item.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                        sources.Add(url);
                }
            }

            return new ChatResponse { Answer = answer, Sources = sources };
        }
        catch
        {
            // Fallback: return the raw text as the answer if JSON parsing fails
            return new ChatResponse { Answer = raw, Sources = [] };
        }
    }
}
