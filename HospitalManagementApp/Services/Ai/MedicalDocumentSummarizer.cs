using Microsoft.Extensions.AI;

namespace HospitalManagementApp.Services.Ai;

public sealed class MedicalDocumentSummarizer : IMedicalDocumentSummarizer
{
    private const int MaxInputCharacters = 6000;
    private readonly IChatClient _chatClient;

    public MedicalDocumentSummarizer(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<MedicalSummaryResult> SummarizeAsync(string text, CancellationToken cancellationToken = default)
    {
        var normalizedText = NormalizeInput(text);

        if (normalizedText.Length < 20)
        {
            throw new ArgumentException("Text must contain at least 20 characters.", nameof(text));
        }

        if (normalizedText.Length > MaxInputCharacters)
        {
            throw new ArgumentException($"Text must not exceed {MaxInputCharacters} characters.", nameof(text));
        }

        var messages = new[]
        {
            new ChatMessage(ChatRole.System,
                "You summarize medical documentation for clinicians. " +
                "Do not diagnose, prescribe, or add facts not present in the input. " +
                "Use concise, neutral language. Return 3 to 5 short bullet points. " +
                "If the input is unclear, say what is unclear."),
            new ChatMessage(ChatRole.User,
                "Summarize the following free-text medical documentation. " +
                "The user is responsible for removing any real patient identifiers before sending it.\n\n" +
                normalizedText)
        };

        var response = await _chatClient.GetResponseAsync(
            messages,
            new ChatOptions
            {
                Temperature = 0.2f,
                MaxOutputTokens = 350
            },
            cancellationToken);

        return new MedicalSummaryResult
        {
            Summary = string.IsNullOrWhiteSpace(response.Text)
                ? "No summary was returned by the AI provider."
                : response.Text.Trim()
        };
    }

    private static string NormalizeInput(string text)
    {
        return string.Join(
            "\n",
            (text ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0));
    }
}
