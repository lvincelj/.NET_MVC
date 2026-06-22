using System.ComponentModel;
using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HospitalManagementApp.Mcp.Tools;

[McpServerToolType]
public static class HospitalReadOnlyTools
{
    private const int MaxLimit = 25;

    [McpServerTool(Name = "search_patients", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Search hospital patients by name and optional age range. Returns only minimal identifying summary fields.")]
    public static async Task<object> SearchPatients(
        AppDbContext db,
        [Description("Optional patient name fragment, for example 'Ana' or 'Kovac'.")] string? query = null,
        [Description("Optional minimum age in years.")] int? minimumAge = null,
        [Description("Optional maximum age in years.")] int? maximumAge = null,
        [Description("Maximum number of patients to return. The server caps this at 25.")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var safeLimit = NormalizeLimit(limit);

        var patientsQuery = db.Patients
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            patientsQuery = patientsQuery.Where(p =>
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.FirstName + " " + p.LastName).Contains(term));
        }

        var patients = await patientsQuery
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.Gender,
                p.DateOfBirth,
                UpcomingAppointments = p.Appointments.Count(a => a.ScheduledAt >= today),
                MedicalRecordCount = p.MedicalRecords.Count
            })
            .ToListAsync(cancellationToken);

        var filtered = patients
            .Select(p => new
            {
                p.Id,
                Name = FullName(p.FirstName, p.LastName),
                Gender = p.Gender.ToString(),
                Age = CalculateAge(p.DateOfBirth, today),
                p.UpcomingAppointments,
                p.MedicalRecordCount
            })
            .Where(p => !minimumAge.HasValue || p.Age >= minimumAge.Value)
            .Where(p => !maximumAge.HasValue || p.Age <= maximumAge.Value)
            .Take(safeLimit)
            .ToList();

        return new
        {
            Count = filtered.Count,
            Patients = filtered
        };
    }

    [McpServerTool(Name = "get_patient_by_id", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Get a minimal read-only patient summary by id, including upcoming appointments and record counts.")]
    public static async Task<object> GetPatientById(
        AppDbContext db,
        [Description("The patient id from the Hospital Management app.")] int patientId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var patient = await db.Patients
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.Gender,
                p.DateOfBirth,
                UpcomingAppointments = p.Appointments
                    .Where(a => a.ScheduledAt >= today)
                    .OrderBy(a => a.ScheduledAt)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Id,
                        a.ScheduledAt,
                        a.Status,
                        a.Room,
                        Doctor = a.Doctor.FirstName + " " + a.Doctor.LastName
                    }),
                MedicalRecordCount = p.MedicalRecords.Count,
                LatestDiagnosis = p.MedicalRecords
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.Diagnosis)
                    .FirstOrDefault(),
                PrescriptionCount = p.MedicalRecords.SelectMany(r => r.Prescriptions).Count(),
                MedicationCount = p.MedicalRecords
                    .SelectMany(r => r.Prescriptions)
                    .SelectMany(pr => pr.Medications)
                    .Count()
            })
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
        {
            return new { Found = false, Message = $"Patient {patientId} was not found." };
        }

        return new
        {
            Found = true,
            Patient = new
            {
                patient.Id,
                Name = FullName(patient.FirstName, patient.LastName),
                Gender = patient.Gender.ToString(),
                Age = CalculateAge(patient.DateOfBirth, today),
                patient.UpcomingAppointments,
                patient.MedicalRecordCount,
                patient.LatestDiagnosis,
                patient.PrescriptionCount,
                patient.MedicationCount
            }
        };
    }

    [McpServerTool(Name = "get_appointments_for_doctor", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Find appointments for a doctor in a date range. Returns appointment status, room, patient name, and doctor name.")]
    public static async Task<object> GetAppointmentsForDoctor(
        AppDbContext db,
        [Description("Doctor name fragment, for example 'Kovac'.")] string doctorName,
        [Description("Inclusive start date in yyyy-MM-dd format. Defaults to today.")] string? dateFrom = null,
        [Description("Inclusive end date in yyyy-MM-dd format. Defaults to seven days after the start date.")] string? dateTo = null,
        [Description("Optional appointment status, for example Scheduled, Completed, Cancelled.")] string? status = null,
        [Description("Maximum number of appointments to return. The server caps this at 25.")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = NormalizeLimit(limit);
        var from = ParseDateOrDefault(dateFrom, DateTime.UtcNow.Date);
        var to = ParseDateOrDefault(dateTo, from.AddDays(7)).Date.AddDays(1).AddTicks(-1);
        var doctorTerm = (doctorName ?? string.Empty).Trim();

        var query = db.Appointments
            .AsNoTracking()
            .Where(a => a.ScheduledAt >= from && a.ScheduledAt <= to);

        if (!string.IsNullOrWhiteSpace(doctorTerm))
        {
            query = query.Where(a =>
                a.Doctor.FirstName.Contains(doctorTerm) ||
                a.Doctor.LastName.Contains(doctorTerm) ||
                (a.Doctor.FirstName + " " + a.Doctor.LastName).Contains(doctorTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<AppointmentStatus>(status.Trim(), ignoreCase: true, out var parsedStatus))
            {
                return new
                {
                    Count = 0,
                    From = from.Date,
                    To = to.Date,
                    Message = $"Unknown appointment status '{status}'.",
                    Appointments = Array.Empty<object>()
                };
            }

            query = query.Where(a => a.Status == parsedStatus);
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledAt)
            .Take(safeLimit)
            .Select(a => new
            {
                a.Id,
                a.ScheduledAt,
                a.Status,
                a.Room,
                Patient = a.Patient.FirstName + " " + a.Patient.LastName,
                Doctor = a.Doctor.FirstName + " " + a.Doctor.LastName
            })
            .ToListAsync(cancellationToken);

        return new
        {
            Count = appointments.Count,
            From = from.Date,
            To = to.Date,
            Appointments = appointments
        };
    }

    [McpServerTool(Name = "search_doctors", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Search doctors by name, specialty, or department. Returns minimal doctor profile and upcoming appointment count.")]
    public static async Task<object> SearchDoctors(
        AppDbContext db,
        [Description("Optional doctor name fragment.")] string? name = null,
        [Description("Optional specialty fragment.")] string? specialty = null,
        [Description("Optional department name fragment.")] string? department = null,
        [Description("Maximum number of doctors to return. The server caps this at 25.")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var safeLimit = NormalizeLimit(limit);

        var query = db.Doctors
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim();
            query = query.Where(d =>
                d.FirstName.Contains(term) ||
                d.LastName.Contains(term) ||
                (d.FirstName + " " + d.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var term = specialty.Trim();
            query = query.Where(d => d.Specialty.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var term = department.Trim();
            query = query.Where(d => d.Departments.Any(dep => dep.Name.Contains(term)));
        }

        var doctors = await query
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Take(safeLimit)
            .Select(d => new
            {
                d.Id,
                Name = d.FirstName + " " + d.LastName,
                d.Specialty,
                Departments = d.Departments.OrderBy(dep => dep.Name).Select(dep => dep.Name),
                UpcomingAppointments = d.Appointments.Count(a => a.ScheduledAt >= today)
            })
            .ToListAsync(cancellationToken);

        return new
        {
            Count = doctors.Count,
            Doctors = doctors
        };
    }

    [McpServerTool(Name = "get_free_beds_by_department", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Return department lookup information for bed-capacity questions. The current app has no bed table, so free-bed counts are not invented.")]
    public static async Task<object> GetFreeBedsByDepartment(
        AppDbContext db,
        [Description("Department name fragment, for example 'Cardiology'.")] string departmentName,
        CancellationToken cancellationToken = default)
    {
        var term = (departmentName ?? string.Empty).Trim();

        var query = db.Departments
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(d => d.Name.Contains(term));
        }

        var departments = await query
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Location,
                d.HeadOfDepartment,
                DoctorCount = d.Doctors.Count,
                FreeBeds = (int?)null
            })
            .ToListAsync(cancellationToken);

        return new
        {
            Count = departments.Count,
            Note = "This Hospital Management app currently does not model beds or bed occupancy, so free-bed counts are unavailable.",
            Departments = departments
        };
    }

    private static int NormalizeLimit(int limit)
    {
        if (limit < 1)
        {
            return 10;
        }

        return Math.Min(limit, MaxLimit);
    }

    private static int CalculateAge(DateTime dateOfBirth, DateTime today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static string FullName(string firstName, string lastName) => $"{firstName} {lastName}".Trim();

    private static DateTime ParseDateOrDefault(string? value, DateTime fallback)
    {
        return DateTime.TryParse(value, out var parsed)
            ? parsed.Date
            : fallback.Date;
    }
}
