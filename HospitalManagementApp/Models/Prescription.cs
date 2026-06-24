using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HospitalManagementApp.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a medical record.")]
        public int MedicalRecordId { get; set; }

        [Required]
        public DateTime IssuedAt { get; set; }

        [Required]
        [StringLength(120)]
        public string IssuedBy { get; set; } = string.Empty;

        [ValidateNever]
        public virtual ICollection<Medication> Medications { get; set; } = new HashSet<Medication>();

        [ForeignKey(nameof(MedicalRecordId))]
        [ValidateNever]
        public virtual MedicalRecord MedicalRecord { get; set; } = null!;
    }
}
