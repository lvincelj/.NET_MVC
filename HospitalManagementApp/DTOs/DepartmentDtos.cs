using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? HeadOfDepartment { get; set; }
    public ICollection<DoctorSummaryDto> Doctors { get; set; } = new List<DoctorSummaryDto>();
}

public class CreateDepartmentDto
{
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

    public ICollection<int> DoctorIds { get; set; } = new List<int>();
}

public class UpdateDepartmentDto
{
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

    public ICollection<int> DoctorIds { get; set; } = new List<int>();
}
