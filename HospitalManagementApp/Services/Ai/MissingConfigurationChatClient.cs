using Microsoft.Extensions.AI;

namespace HospitalManagementApp.Services.Ai;

public sealed class MissingConfigurationChatClient : IChatClient
{
    private const string Message = "AI data assistant is not configured. Set AI:OpenAI:ApiKey in User Secrets or OPENAI_API_KEY.";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new AiConfigurationException(Message);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new AiConfigurationException(Message);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose()
    {
    }
}
