using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class PatientsApiTests : ApiTestBase
{
    public PatientsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Patients_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        var getAll = await Client.GetAsync("/api/Patients");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            firstName = "Ana",
            lastName = "Test",
            gender = 2,
            dateOfBirth = new DateTime(1998, 5, 20),
            email = "ana.test@local.dev",
            phoneNumber = "+385123123",
            address = "Test Street 1"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Patients", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var patientId = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Patients/{patientId}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            firstName = "Ana Updated",
            lastName = "Test",
            gender = 2,
            dateOfBirth = new DateTime(1998, 5, 20),
            email = "ana.updated@local.dev",
            phoneNumber = "+385456456",
            address = "Updated Street 2"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Patients/{patientId}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Patients/{patientId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Patients_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        var getById = await Client.GetAsync("/api/Patients/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            firstName = "Valid",
            lastName = "Payload",
            gender = 1,
            dateOfBirth = new DateTime(1990, 1, 1),
            email = "valid@local.dev",
            phoneNumber = "+385999999",
            address = "Somewhere"
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Patients/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Patients/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Patients_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        var invalidPayload = new
        {
            firstName = "",
            lastName = "",
            gender = 0,
            dateOfBirth = default(DateTime)
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Patients", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var validCreatePayload = new
        {
            firstName = "Tom",
            lastName = "Existing",
            gender = 1,
            dateOfBirth = new DateTime(1992, 7, 10),
            email = "tom.existing@local.dev"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Patients", validCreatePayload);
        var patientId = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Patients/{patientId}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
