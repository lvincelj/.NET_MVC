using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class DoctorsApiTests : ApiTestBase
{
    public DoctorsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Doctors_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        var getAll = await Client.GetAsync("/api/Doctors");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            firstName = "Greg",
            lastName = "House",
            gender = 1,
            specialty = "Internal Medicine",
            email = "greg.house@local.dev",
            phoneNumber = "+385111222",
            departmentIds = Array.Empty<int>()
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Doctors", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Doctors/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            firstName = "Gregory",
            lastName = "House",
            gender = 1,
            specialty = "Diagnostics",
            email = "gregory.house@local.dev",
            phoneNumber = "+385333444",
            departmentIds = Array.Empty<int>()
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Doctors/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Doctors/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Doctors_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        var getById = await Client.GetAsync("/api/Doctors/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            firstName = "Valid",
            lastName = "Doctor",
            gender = 1,
            specialty = "Surgery",
            departmentIds = Array.Empty<int>()
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Doctors/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Doctors/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Doctors_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        var invalidPayload = new
        {
            firstName = "",
            lastName = "",
            gender = 0,
            specialty = "",
            departmentIds = Array.Empty<int>()
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Doctors", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var validCreatePayload = new
        {
            firstName = "James",
            lastName = "Wilson",
            gender = 1,
            specialty = "Oncology",
            departmentIds = Array.Empty<int>()
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Doctors", validCreatePayload);
        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Doctors/{id}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
