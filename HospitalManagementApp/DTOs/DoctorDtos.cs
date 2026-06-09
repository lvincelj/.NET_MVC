using System.ComponentModel.DataAnnotations;
using HospitalManagementApp.Models;

namespace HospitalManagementApp.DTOs;

public class DoctorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public ICollection<DepartmentSummaryDto> Departments { get; set; } = new List<DepartmentSummaryDto>();
    public ICollection<AppointmentSummaryDto> Appointments { get; set; } = new List<AppointmentSummaryDto>();
}

public class CreateDoctorDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a gender.")]
    public Gender Gender { get; set; }

    [Required]
    [StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(120)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public ICollection<int> DepartmentIds { get; set; } = new List<int>();
}

public class UpdateDoctorDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a gender.")]
    public Gender Gender { get; set; }

    [Required]
    [StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(120)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public ICollection<int> DepartmentIds { get; set; } = new List<int>();
}
