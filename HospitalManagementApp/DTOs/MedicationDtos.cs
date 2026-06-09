using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.DTOs;

public class MedicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public int PrescriptionId { get; set; }
    public PrescriptionSummaryDto? Prescription { get; set; }
}

public class CreateMedicationDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Dosage { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Instructions { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a prescription.")]
    public int PrescriptionId { get; set; }
}

public class UpdateMedicationDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Dosage { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Instructions { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a prescription.")]
    public int PrescriptionId { get; set; }
}
