using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class PrescriptionsApiTests : ApiTestBase
{
    public PrescriptionsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Prescriptions_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        var getAll = await Client.GetAsync("/api/Prescriptions");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            medicalRecordId = recordId,
            issuedAt = DateTime.UtcNow,
            issuedBy = "Dr. Prescriber"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Prescriptions", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Prescriptions/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            medicalRecordId = recordId,
            issuedAt = DateTime.UtcNow,
            issuedBy = "Dr. Updated"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Prescriptions/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Prescriptions/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Prescriptions_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        var getById = await Client.GetAsync("/api/Prescriptions/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            medicalRecordId = recordId,
            issuedAt = DateTime.UtcNow,
            issuedBy = "Valid"
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Prescriptions/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Prescriptions/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Prescriptions_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        int prescriptionId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
        });

        var invalidPayload = new
        {
            medicalRecordId = recordId,
            issuedAt = DateTime.UtcNow,
            issuedBy = ""
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Prescriptions", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Prescriptions/{prescriptionId}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
