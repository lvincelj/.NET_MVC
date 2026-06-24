using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HospitalManagementApp.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a patient.")]
        public int PatientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }

        [Required]
        [StringLength(50)]
        public string Room { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(PatientId))]
        [ValidateNever]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(DoctorId))]
        [ValidateNever]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
