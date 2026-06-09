using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.DTOs;

public class PrescriptionDto
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public DateTime IssuedAt { get; set; }
    public string IssuedBy { get; set; } = string.Empty;
    public MedicalRecordSummaryDto? MedicalRecord { get; set; }
    public ICollection<MedicationSummaryDto> Medications { get; set; } = new List<MedicationSummaryDto>();
}

public class CreatePrescriptionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a medical record.")]
    public int MedicalRecordId { get; set; }

    [Required]
    public DateTime IssuedAt { get; set; }

    [Required]
    [StringLength(120)]
    public string IssuedBy { get; set; } = string.Empty;
}

public class UpdatePrescriptionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a medical record.")]
    public int MedicalRecordId { get; set; }

    [Required]
    public DateTime IssuedAt { get; set; }

    [Required]
    [StringLength(120)]
    public string IssuedBy { get; set; } = string.Empty;
}
