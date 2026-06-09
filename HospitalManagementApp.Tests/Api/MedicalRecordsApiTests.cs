using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class MedicalRecordsApiTests : ApiTestBase
{
    public MedicalRecordsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MedicalRecords_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
        });

        var getAll = await Client.GetAsync("/api/MedicalRecords");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            patientId,
            createdAt = DateTime.UtcNow,
            diagnosis = "Test diagnosis",
            notes = "Test notes"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/MedicalRecords", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/MedicalRecords/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            patientId,
            createdAt = DateTime.UtcNow,
            diagnosis = "Updated diagnosis",
            notes = "Updated notes"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/MedicalRecords/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/MedicalRecords/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task MedicalRecords_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
        });

        var getById = await Client.GetAsync("/api/MedicalRecords/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            patientId,
            createdAt = DateTime.UtcNow,
            diagnosis = "Valid",
            notes = "Valid"
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/MedicalRecords/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/MedicalRecords/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task MedicalRecords_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        var invalidPayload = new
        {
            patientId,
            createdAt = DateTime.UtcNow,
            diagnosis = "",
            notes = "Invalid"
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/MedicalRecords", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/MedicalRecords/{recordId}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
