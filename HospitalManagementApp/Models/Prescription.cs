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

        public virtual ICollection<Medication> Medications { get; set; } = new HashSet<Medication>();

        [ForeignKey(nameof(MedicalRecordId))]
        public virtual MedicalRecord MedicalRecord { get; set; } = null!;
    }
}
