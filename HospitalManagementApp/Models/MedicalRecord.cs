using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementApp.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }

        public List<Prescription> Prescriptions { get; set; } = new();

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;
    }
}
