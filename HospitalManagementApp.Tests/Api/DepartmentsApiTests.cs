using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class DepartmentsApiTests : ApiTestBase
{
    public DepartmentsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Departments_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        var getAll = await Client.GetAsync("/api/Departments");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            name = "Cardiology",
            location = "Block C",
            phoneNumber = "+385100200",
            headOfDepartment = "Dr. Head",
            doctorIds = Array.Empty<int>()
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Departments", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Departments/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            name = "Cardiology Updated",
            location = "Block D",
            phoneNumber = "+385100201",
            headOfDepartment = "Dr. New Head",
            doctorIds = Array.Empty<int>()
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Departments/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Departments/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Departments_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        var getById = await Client.GetAsync("/api/Departments/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            name = "Valid",
            location = "Valid",
            phoneNumber = "+385500500",
            doctorIds = Array.Empty<int>()
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Departments/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Departments/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Departments_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        var invalidPayload = new
        {
            name = "",
            location = "",
            doctorIds = Array.Empty<int>()
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Departments", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var validCreatePayload = new
        {
            name = "Neurology",
            location = "Block A",
            doctorIds = Array.Empty<int>()
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Departments", validCreatePayload);
        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Departments/{id}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
