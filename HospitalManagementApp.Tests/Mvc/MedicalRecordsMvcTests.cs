using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HospitalManagementApp.Data;
using HospitalManagementApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HospitalManagementApp.Tests.Mvc;

public class MedicalRecordsMvcTests : ApiTestBase
{
    public MedicalRecordsMvcTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MedicalRecordDeletePage_ReturnsHtmlView_NotApiJson()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        var response = await Client.GetAsync($"/MedicalRecords/Delete/{recordId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Delete Medical Record", body);
        Assert.Contains($"/MedicalRecords/Delete/{recordId}", body);
        Assert.DoesNotContain("\"diagnosis\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MedicalRecordDeletePost_RemovesRecordAndRedirectsToIndex()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);

        var getResponse = await client.GetAsync($"/MedicalRecords/Delete/{recordId}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(getBody);

        var postResponse = await client.PostAsync(
            $"/MedicalRecords/Delete/{recordId}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = recordId.ToString(),
                ["__RequestVerificationToken"] = antiForgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/MedicalRecords", postResponse.Headers.Location?.OriginalString);

        await Factory.ExecuteDbContextAsync(async context =>
        {
            var exists = await context.MedicalRecords.AnyAsync(r => r.Id == recordId);
            Assert.False(exists);
        });
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"(?<value>[^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Anti-forgery token was not rendered on the delete form.");
        return WebUtility.HtmlDecode(match.Groups["value"].Value);
    }
}
