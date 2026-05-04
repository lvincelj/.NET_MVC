using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementApp.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MedicalRecordId { get; set; }

        [Required]
        public DateTime IssuedAt { get; set; }

        [Required]
        [StringLength(120)]
        public string IssuedBy { get; set; } = string.Empty;

        public List<Medication> Medications { get; set; } = new();

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord MedicalRecord { get; set; } = null!;
    }
}
