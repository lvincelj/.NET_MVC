using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HospitalManagementApp.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a gender.")]
        public Gender Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [EmailAddress]
        [StringLength(120)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [ValidateNever]
        public virtual ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        [ValidateNever]
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new HashSet<MedicalRecord>();

        [ValidateNever]
        public virtual ICollection<PacijentDatoteka> PacijentDatoteke { get; set; } = new HashSet<PacijentDatoteka>();
    }
}
