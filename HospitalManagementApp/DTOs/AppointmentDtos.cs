using System.ComponentModel.DataAnnotations;
using HospitalManagementApp.Models;

namespace HospitalManagementApp.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Room { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public PatientSummaryDto? Patient { get; set; }
    public DoctorSummaryDto? Doctor { get; set; }
}

public class CreateAppointmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a patient.")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select appointment status.")]
    public AppointmentStatus Status { get; set; }

    [Required]
    [StringLength(50)]
    public string Room { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a patient.")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select appointment status.")]
    public AppointmentStatus Status { get; set; }

    [Required]
    [StringLength(50)]
    public string Room { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
