using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Services.Ai;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class AiApiTests : ApiTestBase
{
    public AiApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DataAssistant_WithMissingApiKey_Returns503WithoutSendingData()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/data-assistant", new
        {
            question = "appointments tomorrow for Dr. House"
        });
        var body = await response.Content.ReadFromJsonAsync<MissingAiConfigurationResponse>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(body);
        Assert.Contains("AI data assistant is not configured", body.Error);
        Assert.Equal(DataAssistantDisclaimer.Text, body.Disclaimer);
    }

    [Fact]
    public async Task DataAssistantUi_ReturnsHtmlWithVerificationNotice()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync("/ai/data-assistant");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Data Assistant", body);
        Assert.Contains("verified against the official records", body);
    }

    [Fact]
    public async Task DataAssistant_WithShortInput_ReturnsValidationProblem()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/data-assistant", new
        {
            question = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private sealed class MissingAiConfigurationResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = string.Empty;
    }
}
