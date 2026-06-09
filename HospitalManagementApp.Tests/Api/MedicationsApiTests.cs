using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class MedicationsApiTests : ApiTestBase
{
    public MedicationsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Medications_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        int prescriptionId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
        });

        var getAll = await Client.GetAsync("/api/Medications");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            name = "Paracetamol",
            dosage = "500mg",
            instructions = "Twice daily",
            prescriptionId
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Medications", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Medications/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            name = "Paracetamol Updated",
            dosage = "750mg",
            instructions = "After food",
            prescriptionId
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Medications/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Medications/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Medications_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        int prescriptionId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
        });

        var getById = await Client.GetAsync("/api/Medications/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            name = "Valid",
            dosage = "100mg",
            instructions = "Valid",
            prescriptionId
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Medications/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Medications/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Medications_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        int prescriptionId = 0;
        int medicationId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
            medicationId = await TestDataSeeder.CreateMedicationAsync(context, prescriptionId);
        });

        var invalidPayload = new
        {
            name = "",
            dosage = "",
            instructions = "",
            prescriptionId = 0
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Medications", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Medications/{medicationId}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
