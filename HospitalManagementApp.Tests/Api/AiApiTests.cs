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
    public async Task MedicalSummary_WithMissingApiKey_Returns503WithoutSendingData()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/medical-summary", new
        {
            text = "Patient details have been removed. Notes mention fever, cough, follow-up plan, and medication review."
        });
        var body = await response.Content.ReadFromJsonAsync<MissingAiConfigurationResponse>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(body);
        Assert.Contains("AI summarization is not configured", body.Error);
        Assert.Equal(MedicalSummaryDisclaimer.Text, body.Disclaimer);
    }

    [Fact]
    public async Task MedicalSummaryUi_ReturnsHtmlWithPrivacyNotice()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync("/ai/medical-summary");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("AI Medical Summary", body);
        Assert.Contains("Remove names, identifiers", body);
    }

    [Fact]
    public async Task MedicalSummary_WithShortInput_ReturnsValidationProblem()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/medical-summary", new
        {
            text = "Too short"
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
