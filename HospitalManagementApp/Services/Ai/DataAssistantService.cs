using Microsoft.Extensions.AI;

namespace HospitalManagementApp.Services.Ai;

public sealed class DataAssistantService : IDataAssistantService
{
    private readonly IChatClient _chatClient;
    private readonly IDataAssistantToolProvider _toolProvider;

    public DataAssistantService(IChatClient chatClient, IDataAssistantToolProvider toolProvider)
    {
        _chatClient = chatClient;
        _toolProvider = toolProvider;
    }

    public async Task<DataAssistantResult> AnswerAsync(string question, CancellationToken cancellationToken = default)
    {
        var normalizedQuestion = NormalizeQuestion(question);
        if (normalizedQuestion.Length < 3)
        {
            throw new ArgumentException("Question must contain at least 3 characters.", nameof(question));
        }

        var messages = new[]
        {
            new ChatMessage(ChatRole.System,
                "You are a read-only hospital data assistant. Answer staff questions using only tool results from the app database. " +
                "Never invent patient, appointment, doctor, department, medication, prescription, or record data. " +
                "If a tool returns no data, say no matching records were found. " +
                "If the question asks for data not tracked by the app, say that the app does not currently track it. " +
                "Do not reveal phone numbers, emails, addresses, or free-text clinical notes. " +
                "Keep answers concise and plain-language, with dates and counts when available."),
            new ChatMessage(ChatRole.User, normalizedQuestion)
        };

        var response = await _chatClient.GetResponseAsync(
            messages,
            new ChatOptions
            {
                Temperature = 0.1f,
                MaxOutputTokens = 500,
                ToolMode = ChatToolMode.Auto,
                Tools = _toolProvider.CreateTools()
            },
            cancellationToken);

        return new DataAssistantResult
        {
            Answer = string.IsNullOrWhiteSpace(response.Text)
                ? "I could not produce an answer from the available app data."
                : response.Text.Trim()
        };
    }

    private static string NormalizeQuestion(string question)
    {
        return string.Join(
            " ",
            (question ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
