using System.Net;
using System.Net.Http.Json;
using HospitalManagementApp.Services.Ai;
using HospitalManagementApp.Tests.Infrastructure;
using Xunit;

namespace HospitalManagementApp.Tests.Api;

public class AiApiTests : ApiTestBase
{
    public AiApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DataAssistant_WithMissingApiKey_Returns503WithoutSendingData()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/data-assistant", new
        {
            question = "appointments tomorrow for Dr. House"
        });
        var body = await response.Content.ReadFromJsonAsync<MissingAiConfigurationResponse>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(body);
        Assert.Contains("AI data assistant is not configured", body.Error);
        Assert.Equal(DataAssistantDisclaimer.Text, body.Disclaimer);
    }

    [Fact]
    public async Task DataAssistantUi_ReturnsHtmlWithVerificationNotice()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync("/ai/data-assistant");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Data Assistant", body);
        Assert.Contains("verified against the official records", body);
    }

    [Fact]
    public async Task DataAssistant_WithShortInput_ReturnsValidationProblem()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync("/api/ai/data-assistant", new
        {
            question = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DataAssistantTools_CareMapReadsAcrossRelatedTables()
    {
        await ResetDatabaseAsync();

        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var departmentId = await TestDataSeeder.CreateDepartmentAsync(context);
            var doctorId = await TestDataSeeder.CreateDoctorAsync(context, departmentId);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            var prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
            await TestDataSeeder.CreateMedicationAsync(context, prescriptionId);
            await TestDataSeeder.CreateAppointmentAsync(context, patientId, doctorId);

            var provider = new DataAssistantToolProvider(context);
            var result = await provider.GetPatientCareMapAsync(patientId);
            var patient = Assert.Single(result.Items);
            var appointment = Assert.Single(patient.Appointments);
            var medicalRecord = Assert.Single(patient.MedicalRecords);
            var prescription = Assert.Single(medicalRecord.Prescriptions);
            var medication = Assert.Single(prescription.Medications);

            Assert.Equal("John Tester", patient.Name);
            Assert.Equal("Greg House", appointment.Doctor[..10]);
            Assert.Contains("Diagnostics", appointment.DoctorSpecialty);
            Assert.Equal("Test diagnosis", medicalRecord.Diagnosis);
            Assert.Equal("Ibuprofen", medication.Name);
        });
    }

    [Fact]
    public async Task DataAssistantTools_GeneratesPatientSummaryDocument()
    {
        await ResetDatabaseAsync();

        await Factory.ExecuteDbContextAsync(async context =>
        {
            var patientId = await TestDataSeeder.CreatePatientAsync(context);
            var departmentId = await TestDataSeeder.CreateDepartmentAsync(context);
            var doctorId = await TestDataSeeder.CreateDoctorAsync(context, departmentId);
            var recordId = await TestDataSeeder.CreateMedicalRecordAsync(context, patientId);
            var prescriptionId = await TestDataSeeder.CreatePrescriptionAsync(context, recordId);
            await TestDataSeeder.CreateMedicationAsync(context, prescriptionId);
            await TestDataSeeder.CreateAppointmentAsync(context, patientId, doctorId);

            var provider = new DataAssistantToolProvider(context);
            var document = await provider.GeneratePatientSummaryDocumentAsync(patientId);

            Assert.Equal("generate_patient_summary_document", document.Tool);
            Assert.Contains("Patient Summary - John Tester", document.Content);
            Assert.Contains("Appointments", document.Content);
            Assert.Contains("Medical Records, Prescriptions, and Medications", document.Content);
            Assert.Contains(DataAssistantDisclaimer.Text, document.Disclaimer);
        });
    }

    private sealed class MissingAiConfigurationResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = string.Empty;
    }
}
