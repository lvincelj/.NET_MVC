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

    [Theory]
    [InlineData("Appointments", "Delete Appointment")]
    [InlineData("Departments", "Delete Department")]
    [InlineData("Doctors", "Delete Doctor")]
    [InlineData("MedicalRecords", "Delete Medical Record")]
    [InlineData("Medications", "Delete Medication")]
    [InlineData("Patients", "Delete Patient")]
    [InlineData("Prescriptions", "Delete Prescription")]
    public async Task EntityDeletePages_ReturnHtmlView_NotApiJson(string controller, string expectedTitle)
    {
        await ResetDatabaseAsync();

        var id = await CreateEntityForControllerAsync(controller);

        var response = await Client.GetAsync($"/{controller}/Delete/{id}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(expectedTitle, body);
        Assert.DoesNotContain("{\"", body);
    }

    [Fact]
    public async Task PatientDeletePost_RemovesNestedMedicalData()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        int doctorId = 0;
        int recordId = 0;
        int prescriptionId = 0;
        int medicationId = 0;
        int appointmentId = 0;

        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
            doctorId = await TestDataSeeder.CreateDoctorAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
            medicationId = await TestDataSeeder.CreateMedicationAsync(context, prescriptionId);
            appointmentId = await TestDataSeeder.CreateAppointmentAsync(context, patientId, doctorId);
        });

        using var client = CreateNoRedirectClient();
        var getResponse = await client.GetAsync($"/Patients/Delete/{patientId}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(getBody);

        var postResponse = await client.PostAsync(
            $"/Patients/Delete/{patientId}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = patientId.ToString(),
                ["__RequestVerificationToken"] = antiForgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/patients", postResponse.Headers.Location?.OriginalString);

        await Factory.ExecuteDbContextAsync(async context =>
        {
            Assert.False(await context.Patients.AnyAsync(p => p.Id == patientId));
            Assert.False(await context.Appointments.AnyAsync(a => a.Id == appointmentId));
            Assert.False(await context.MedicalRecords.AnyAsync(r => r.Id == recordId));
            Assert.False(await context.Prescriptions.AnyAsync(p => p.Id == prescriptionId));
            Assert.False(await context.Medications.AnyAsync(m => m.Id == medicationId));
            Assert.True(await context.Doctors.AnyAsync(d => d.Id == doctorId));
        });
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

        using var client = CreateNoRedirectClient();

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

    private HttpClient CreateNoRedirectClient()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        return client;
    }

    private async Task<int> CreateEntityForControllerAsync(string controller)
    {
        var id = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            switch (controller)
            {
                case "Appointments":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        var doctorId = await TestDataSeeder.CreateDoctorAsync(context);
                        id = await TestDataSeeder.CreateAppointmentAsync(context, patientId, doctorId);
                        break;
                    }
                case "Departments":
                    id = await TestDataSeeder.CreateDepartmentAsync(context);
                    break;
                case "Doctors":
                    id = await TestDataSeeder.CreateDoctorAsync(context);
                    break;
                case "MedicalRecords":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        id = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
                        break;
                    }
                case "Medications":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
                        var prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
                        id = await TestDataSeeder.CreateMedicationAsync(context, prescriptionId);
                        break;
                    }
                case "Patients":
                    id = await TestDataSeeder.CreatePatientAsync(context);
                    break;
                case "Prescriptions":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
                        id = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
                        break;
                    }
            }
        });

        return id;
    }
}
