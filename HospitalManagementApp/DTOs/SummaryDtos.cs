namespace HospitalManagementApp.DTOs;

public class DepartmentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DoctorSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
}

public class PatientSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Room { get; set; } = string.Empty;
}

public class MedicalRecordSummaryDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
}

public class PrescriptionSummaryDto
{
    public int Id { get; set; }
    public DateTime IssuedAt { get; set; }
    public string IssuedBy { get; set; } = string.Empty;
}

public class MedicationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
}
