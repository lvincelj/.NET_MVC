namespace HospitalManagementApp.Services.Ai;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public OpenAiOptions OpenAI { get; set; } = new();
}

public sealed class OpenAiOptions
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
}
