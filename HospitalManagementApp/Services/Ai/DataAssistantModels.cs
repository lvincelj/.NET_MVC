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

public sealed record PatientCareMapToolItem(
    int Id,
    string Name,
    int Age,
    string Gender,
    IReadOnlyList<CareMapAppointmentItem> Appointments,
    IReadOnlyList<CareMapMedicalRecordItem> MedicalRecords);

public sealed record CareMapAppointmentItem(
    int Id,
    string ScheduledAt,
    string Status,
    string Room,
    string Doctor,
    string DoctorSpecialty,
    IReadOnlyList<string> Departments);

public sealed record CareMapMedicalRecordItem(
    int Id,
    string CreatedAt,
    string Diagnosis,
    IReadOnlyList<CareMapPrescriptionItem> Prescriptions);

public sealed record CareMapPrescriptionItem(
    int Id,
    string IssuedAt,
    string IssuedBy,
    IReadOnlyList<CareMapMedicationItem> Medications);

public sealed record CareMapMedicationItem(
    int Id,
    string Name,
    string Dosage,
    string? Instructions);

public sealed record DocumentToolResult(
    string Tool,
    string Criteria,
    string Title,
    string Content,
    string Disclaimer);
