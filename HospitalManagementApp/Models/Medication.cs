namespace HospitalManagementApp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Medication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string Dosage { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Instructions { get; set; }

        [Required]
        public int PrescriptionId { get; set; }

        [ForeignKey(nameof(PrescriptionId))]
        public virtual Prescription Prescription { get; set; } = null!;
    }
}
