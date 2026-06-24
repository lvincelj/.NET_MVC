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

    [Theory]
    [InlineData("Appointments")]
    [InlineData("Departments")]
    [InlineData("Doctors")]
    [InlineData("MedicalRecords")]
    [InlineData("Medications")]
    [InlineData("Patients")]
    [InlineData("Prescriptions")]
    public async Task EntityIndexPages_RenderMvcDeleteLinks_NotApiLinks(string controller)
    {
        await ResetDatabaseAsync();

        var id = await CreateEntityForControllerAsync(controller);

        var response = await Client.GetAsync($"/{controller}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"/{controller}/Delete", body);
        Assert.DoesNotContain($"/api/{controller}/{id}", body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Patients", "Create Patient")]
    [InlineData("Departments", "Create Department")]
    [InlineData("Appointments", "Create Appointment")]
    [InlineData("Medications", "Create Medication")]
    [InlineData("Prescriptions", "Create Prescription")]
    public async Task EntityCreateLinks_OpenMvcHtmlViews_NotApiJson(string controller, string expectedTitle)
    {
        await ResetDatabaseAsync();

        var indexResponse = await Client.GetAsync($"/{controller}");
        var indexBody = await indexResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
        Assert.Contains($"href=\"/{controller}/Create\"", indexBody);
        Assert.DoesNotContain($"href=\"/api/{controller}\"", indexBody, StringComparison.OrdinalIgnoreCase);

        var createResponse = await Client.GetAsync($"/{controller}/Create");
        var createBody = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Contains("text/html", createResponse.Content.Headers.ContentType?.MediaType);
        Assert.Contains(expectedTitle, createBody);
        Assert.DoesNotContain("{\"", createBody);
    }

    [Theory]
    [InlineData("Appointments")]
    [InlineData("Departments")]
    [InlineData("Doctors")]
    [InlineData("MedicalRecords")]
    [InlineData("Medications")]
    [InlineData("Patients")]
    [InlineData("Prescriptions")]
    public async Task EntityDeletePost_RemovesEntityAndRedirects(string controller)
    {
        await ResetDatabaseAsync();

        var id = await CreateEntityForControllerAsync(controller);
        using var client = CreateNoRedirectClient();

        var getResponse = await client.GetAsync($"/{controller}/Delete/{id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(getBody);

        var postResponse = await client.PostAsync(
            $"/{controller}/Delete/{id}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = id.ToString(),
                ["__RequestVerificationToken"] = antiForgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        await AssertEntityDeletedAsync(controller, id);
    }

    [Theory]
    [InlineData("Patients")]
    [InlineData("Departments")]
    [InlineData("Appointments")]
    [InlineData("Medications")]
    [InlineData("Prescriptions")]
    public async Task EntityCreatePost_CreatesEntityAndRedirects(string controller)
    {
        await ResetDatabaseAsync();

        var form = await BuildCreateFormAsync(controller);
        using var client = CreateNoRedirectClient();

        var getResponse = await client.GetAsync($"/{controller}/Create");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        form["__RequestVerificationToken"] = ExtractAntiForgeryToken(getBody);

        var postResponse = await client.PostAsync($"/{controller}/Create", new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        await AssertEntityCreatedAsync(controller);
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
    public async Task PatientDetails_ReturnsHtmlFromCleanAndLegacyRoutes()
    {
        await ResetDatabaseAsync();

        var patientId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            patientId = await TestDataSeeder.CreatePatientAsync(context);
        });

        var indexResponse = await Client.GetAsync("/Patients");
        var indexBody = await indexResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
        Assert.Contains($"/patients/{patientId}", indexBody);

        foreach (var path in new[] { $"/patients/{patientId}", $"/Patients/Details/{patientId}" })
        {
            var response = await Client.GetAsync(path);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("Patient Details", body);
        }
    }

    [Fact]
    public async Task MedicalRecordDetails_AllowsAdminToAddPrescription()
    {
        await ResetDatabaseAsync();

        int recordId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
        });

        using var client = CreateNoRedirectClient();

        var detailsResponse = await client.GetAsync($"/MedicalRecords/Details/{recordId}");
        var detailsBody = await detailsResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        Assert.Contains($"/Prescriptions/Create?medicalRecordId={recordId}", detailsBody);

        var createResponse = await client.GetAsync($"/Prescriptions/Create?medicalRecordId={recordId}");
        var createBody = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Contains($"value=\"{recordId}\"", createBody);

        var antiForgeryToken = ExtractAntiForgeryToken(createBody);
        var postResponse = await client.PostAsync(
            "/Prescriptions/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["MedicalRecordId"] = recordId.ToString(),
                ["IssuedBy"] = "Admin Doctor",
                ["IssuedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"),
                ["__RequestVerificationToken"] = antiForgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal($"/MedicalRecords/Details/{recordId}", postResponse.Headers.Location?.OriginalString);

        await Factory.ExecuteDbContextAsync(async context =>
        {
            Assert.True(await context.Prescriptions.AnyAsync(p => p.MedicalRecordId == recordId && p.IssuedBy == "Admin Doctor"));
        });
    }

    [Fact]
    public async Task PrescriptionDetails_AllowsAdminToAddMedication()
    {
        await ResetDatabaseAsync();

        int prescriptionId = 0;
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
        });

        using var client = CreateNoRedirectClient();

        var detailsResponse = await client.GetAsync($"/Prescriptions/Details/{prescriptionId}");
        var detailsBody = await detailsResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        Assert.Contains($"/Medications/Create?prescriptionId={prescriptionId}", detailsBody);

        var createResponse = await client.GetAsync($"/Medications/Create?prescriptionId={prescriptionId}");
        var createBody = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Contains($"value=\"{prescriptionId}\"", createBody);

        var antiForgeryToken = ExtractAntiForgeryToken(createBody);
        var postResponse = await client.PostAsync(
            "/Medications/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["PrescriptionId"] = prescriptionId.ToString(),
                ["Name"] = "Admin Medication",
                ["Dosage"] = "20mg",
                ["Instructions"] = "After meal",
                ["__RequestVerificationToken"] = antiForgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal($"/Prescriptions/Details/{prescriptionId}", postResponse.Headers.Location?.OriginalString);

        await Factory.ExecuteDbContextAsync(async context =>
        {
            Assert.True(await context.Medications.AnyAsync(m => m.PrescriptionId == prescriptionId && m.Name == "Admin Medication"));
        });
    }

    [Fact]
    public async Task PrescriptionAndMedicationCreateForms_RenderParentSelectors()
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

        var prescriptionResponse = await Client.GetAsync("/Prescriptions/Create");
        var prescriptionBody = await prescriptionResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, prescriptionResponse.StatusCode);
        Assert.Contains("select", prescriptionBody);
        Assert.Contains("name=\"MedicalRecordId\"", prescriptionBody);
        Assert.Contains($"value=\"{recordId}\"", prescriptionBody);
        Assert.DoesNotContain("hm-ajax-dropdown", prescriptionBody);

        var medicationResponse = await Client.GetAsync("/Medications/Create");
        var medicationBody = await medicationResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, medicationResponse.StatusCode);
        Assert.Contains("select", medicationBody);
        Assert.Contains("name=\"PrescriptionId\"", medicationBody);
        Assert.Contains($"value=\"{prescriptionId}\"", medicationBody);
        Assert.DoesNotContain("hm-ajax-dropdown", medicationBody);
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

        Assert.True(match.Success, "Anti-forgery token was not rendered on the form.");
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

    private async Task<Dictionary<string, string>> BuildCreateFormAsync(string controller)
    {
        var form = new Dictionary<string, string>();

        await Factory.ExecuteDbContextAsync(async context =>
        {
            switch (controller)
            {
                case "Patients":
                    form["FirstName"] = "Create";
                    form["LastName"] = "Patient";
                    form["Gender"] = "1";
                    form["DateOfBirth"] = "1994-03-01";
                    form["Email"] = "create.patient@test.local";
                    form["PhoneNumber"] = "+385111222";
                    form["Address"] = "Create Street 1";
                    break;
                case "Departments":
                    form["Name"] = "Create Department";
                    form["Location"] = "Wing C";
                    form["PhoneNumber"] = "+385333444";
                    form["HeadOfDepartment"] = "Dr. Create Lead";
                    break;
                case "Appointments":
                    form["PatientId"] = (await TestDataSeeder.CreatePatientAsync(context)).ToString();
                    form["DoctorId"] = (await TestDataSeeder.CreateDoctorAsync(context)).ToString();
                    form["ScheduledAt"] = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm");
                    form["Status"] = "1";
                    form["Room"] = "T12";
                    form["Notes"] = "Created through MVC test";
                    break;
                case "Medications":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
                        var prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
                        form["Name"] = "Create Medication";
                        form["Dosage"] = "10mg";
                        form["PrescriptionId"] = prescriptionId.ToString();
                        form["Instructions"] = "After meal";
                        break;
                    }
                case "Prescriptions":
                    {
                        var patientId = await TestDataSeeder.CreatePatientAsync(context);
                        var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
                        form["MedicalRecordId"] = recordId.ToString();
                        form["IssuedBy"] = "Dr. Create";
                        form["IssuedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
                        break;
                    }
            }
        });

        return form;
    }

    private async Task AssertEntityCreatedAsync(string controller)
    {
        await Factory.ExecuteDbContextAsync(async context =>
        {
            switch (controller)
            {
                case "Patients":
                    Assert.True(await context.Patients.AnyAsync(p => p.FirstName == "Create" && p.LastName == "Patient"));
                    break;
                case "Departments":
                    Assert.True(await context.Departments.AnyAsync(d => d.Name == "Create Department"));
                    break;
                case "Appointments":
                    Assert.True(await context.Appointments.AnyAsync(a => a.Room == "T12"));
                    break;
                case "Medications":
                    Assert.True(await context.Medications.AnyAsync(m => m.Name == "Create Medication"));
                    break;
                case "Prescriptions":
                    Assert.True(await context.Prescriptions.AnyAsync(p => p.IssuedBy == "Dr. Create"));
                    break;
            }
        });
    }

    private async Task AssertEntityDeletedAsync(string controller, int id)
    {
        await Factory.ExecuteDbContextAsync(async context =>
        {
            var exists = controller switch
            {
                "Appointments" => await context.Appointments.AnyAsync(a => a.Id == id),
                "Departments" => await context.Departments.AnyAsync(d => d.Id == id),
                "Doctors" => await context.Doctors.AnyAsync(d => d.Id == id),
                "MedicalRecords" => await context.MedicalRecords.AnyAsync(r => r.Id == id),
                "Medications" => await context.Medications.AnyAsync(m => m.Id == id),
                "Patients" => await context.Patients.AnyAsync(p => p.Id == id),
                "Prescriptions" => await context.Prescriptions.AnyAsync(p => p.Id == id),
                _ => true
            };

            Assert.False(exists);
        });
    }
}
