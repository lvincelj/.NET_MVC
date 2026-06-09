using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class AppointmentsApiTests : ApiTestBase
{
    public AppointmentsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Appointments_Crud_ReturnsExpectedStatusCodes()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        int doctorId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
            doctorId = await TestDataSeeder.CreateDoctorAsync(context);
        });

        var getAll = await Client.GetAsync("/api/Appointments");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var createPayload = new
        {
            patientId,
            doctorId,
            scheduledAt = DateTime.UtcNow.AddDays(1),
            status = 1,
            room = "B12",
            notes = "Routine check"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Appointments", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var id = await ApiResponseHelpers.ReadIdAsync(createResponse);

        var getById = await Client.GetAsync($"/api/Appointments/{id}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var updatePayload = new
        {
            patientId,
            doctorId,
            scheduledAt = DateTime.UtcNow.AddDays(2),
            status = 2,
            room = "C21",
            notes = "Updated notes"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/Appointments/{id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync($"/api/Appointments/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Appointments_GetPutDelete_WithMissingId_Returns404()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        int doctorId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
            doctorId = await TestDataSeeder.CreateDoctorAsync(context);
        });

        var getById = await Client.GetAsync("/api/Appointments/999999");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);

        var validUpdatePayload = new
        {
            patientId,
            doctorId,
            scheduledAt = DateTime.UtcNow.AddDays(2),
            status = 1,
            room = "A1",
            notes = "Valid"
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Appointments/999999", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await Client.DeleteAsync("/api/Appointments/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Appointments_PostAndPut_WithInvalidPayload_Returns400()
    {
        await ResetDatabaseAsync();

        int patientId = 0;
        int doctorId = 0;
        int appointmentId = 0;

        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
            doctorId = await TestDataSeeder.CreateDoctorAsync(context);
            appointmentId = await TestDataSeeder.CreateAppointmentAsync(context, patientId, doctorId);
        });

        var invalidPayload = new
        {
            patientId,
            doctorId,
            scheduledAt = DateTime.UtcNow,
            status = 0,
            room = "",
            notes = "Invalid"
        };

        var invalidPostResponse = await Client.PostAsJsonAsync("/api/Appointments", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPostResponse.StatusCode);

        var invalidPutResponse = await Client.PutAsJsonAsync($"/api/Appointments/{appointmentId}", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPutResponse.StatusCode);
    }
}
