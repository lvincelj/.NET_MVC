using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Location { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? HeadOfDepartment { get; set; }

        public List<Doctor> Doctors { get; set; } = new();
    }
}
