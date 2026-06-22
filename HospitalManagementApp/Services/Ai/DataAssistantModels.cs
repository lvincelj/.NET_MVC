using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.Services.Ai;

public sealed class DataAssistantRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Question must be between 3 and 1000 characters.")]
    public string Question { get; set; } = string.Empty;
}

public sealed class DataAssistantResult
{
    public string Answer { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = DataAssistantDisclaimer.Text;
}

public static class DataAssistantDisclaimer
{
    public const string Text = "AI answers are generated from available app data and should be verified against the official records.";
}

public sealed record DataAssistantToolResult<T>(
    string Tool,
    string Criteria,
    int Count,
    IReadOnlyList<T> Items,
    string? Note = null);

public sealed record PatientToolItem(
    int Id,
    string Name,
    int Age,
    string Gender,
    int AppointmentCount,
    int MedicalRecordCount,
    string? NextAppointment);

public sealed record AppointmentToolItem(
    int Id,
    string ScheduledAt,
    string Patient,
    string Doctor,
    string DoctorSpecialty,
    string? Department,
    string Status,
    string Room);

public sealed record DoctorToolItem(
    int Id,
    string Name,
    string Specialty,
    IReadOnlyList<string> Departments,
    int UpcomingAppointments);

public sealed record DepartmentToolItem(
    int Id,
    string Name,
    string Location,
    string? HeadOfDepartment,
    int DoctorCount,
    int UpcomingAppointments,
    string? Note);

public sealed record MedicalRecordToolItem(
    int Id,
    string CreatedAt,
    string Patient,
    string Diagnosis,
    int PrescriptionCount);

public sealed record PrescriptionToolItem(
    int Id,
    string IssuedAt,
    string IssuedBy,
    string Patient,
    string Diagnosis,
    int MedicationCount);

public sealed record MedicationToolItem(
    int Id,
    string Name,
    string Dosage,
    string? Instructions,
    string IssuedBy,
    string Diagnosis);
