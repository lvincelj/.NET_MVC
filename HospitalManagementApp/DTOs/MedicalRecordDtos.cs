using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.DTOs;

public class MedicalRecordDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public PatientSummaryDto? Patient { get; set; }
    public ICollection<PrescriptionSummaryDto> Prescriptions { get; set; } = new List<PrescriptionSummaryDto>();
}

public class CreateMedicalRecordDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a patient.")]
    public int PatientId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateMedicalRecordDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a patient.")]
    public int PatientId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }
}
