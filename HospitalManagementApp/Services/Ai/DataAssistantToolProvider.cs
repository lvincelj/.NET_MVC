using System.ComponentModel;
using System.Text.Json;
using HospitalManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace HospitalManagementApp.Services.Ai;

public sealed class DataAssistantToolProvider : IDataAssistantToolProvider
{
    private const int DefaultLimit = 15;
    private const int MaxLimit = 30;

    private readonly AppDbContext _context;

    public DataAssistantToolProvider(AppDbContext context)
    {
        _context = context;
    }

    public IList<AITool> CreateTools()
    {
        return
        [
            AIFunctionFactory.Create(
                SearchPatientsAsync,
                "search_patients",
                "Read-only search for patients by name, age range, and optional follow-up appointment date range. Returns minimal patient data only.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                SearchAppointmentsAsync,
                "search_appointments",
                "Read-only search for appointments by doctor, patient, department, status, and date range.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                SearchDoctorsAsync,
                "search_doctors",
                "Read-only search for doctors by name, specialty, and department.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                GetDepartmentOverviewAsync,
                "get_department_overview",
                "Read-only department overview. Includes doctors and upcoming appointments. Bed availability is not tracked in this app.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                SearchMedicalRecordsAsync,
                "search_medical_records",
                "Read-only search for medical records by patient, diagnosis, and created date range. Does not return free-text notes.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                SearchPrescriptionsAsync,
                "search_prescriptions",
                "Read-only search for prescriptions by patient, diagnosis, issuer, and issued date range.",
                JsonSerializerOptions.Web),
            AIFunctionFactory.Create(
                SearchMedicationsAsync,
                "search_medications",
                "Read-only search for medications by medication name, dosage, and instructions.",
                JsonSerializerOptions.Web)
        ];
    }

    [Description("Search patients by name, age range, and optional follow-up appointment date range.")]
    public async Task<DataAssistantToolResult<PatientToolItem>> SearchPatientsAsync(
        string? name = null,
        int? minimumAge = null,
        int? maximumAge = null,
        string? followUpFrom = null,
        string? followUpTo = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cappedLimit = CapLimit(limit);
        var from = ParseDate(followUpFrom);
        var to = ParseDate(followUpTo, endOfDay: true);

        var query = _context.Patients
            .AsNoTracking()
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim();
            query = query.Where(p => (p.FirstName + " " + p.LastName).Contains(term));
        }

        if (minimumAge is not null)
        {
            var latestBirthDate = now.AddYears(-minimumAge.Value);
            query = query.Where(p => p.DateOfBirth <= latestBirthDate);
        }

        if (maximumAge is not null)
        {
            var earliestBirthDate = now.AddYears(-(maximumAge.Value + 1)).AddDays(1);
            query = query.Where(p => p.DateOfBirth >= earliestBirthDate);
        }

        if (from is not null)
        {
            query = query.Where(p => p.Appointments.Any(a => a.ScheduledAt >= from.Value));
        }

        if (to is not null)
        {
            query = query.Where(p => p.Appointments.Any(a => a.ScheduledAt <= to.Value));
        }

        var patients = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = patients.Select(p =>
        {
            var nextAppointment = p.Appointments
                .Where(a => a.ScheduledAt >= now)
                .OrderBy(a => a.ScheduledAt)
                .FirstOrDefault();

            return new PatientToolItem(
                p.Id,
                $"{p.FirstName} {p.LastName}",
                CalculateAge(p.DateOfBirth, now),
                p.Gender.ToString(),
                p.Appointments.Count,
                p.MedicalRecords.Count,
                nextAppointment?.ScheduledAt.ToString("yyyy-MM-dd HH:mm"));
        }).ToList();

        return new("search_patients", BuildCriteria((nameof(name), name), (nameof(minimumAge), minimumAge), (nameof(maximumAge), maximumAge), (nameof(followUpFrom), followUpFrom), (nameof(followUpTo), followUpTo)), items.Count, items);
    }

    [Description("Search appointments by doctor, patient, department, status, and date range.")]
    public async Task<DataAssistantToolResult<AppointmentToolItem>> SearchAppointmentsAsync(
        string? doctorName = null,
        string? patientName = null,
        string? departmentName = null,
        string? status = null,
        string? dateFrom = null,
        string? dateTo = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = CapLimit(limit);
        var from = ParseDate(dateFrom);
        var to = ParseDate(dateTo, endOfDay: true);

        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Departments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(doctorName))
        {
            var term = doctorName.Trim();
            query = query.Where(a => (a.Doctor.FirstName + " " + a.Doctor.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var term = patientName.Trim();
            query = query.Where(a => (a.Patient.FirstName + " " + a.Patient.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(departmentName))
        {
            var term = departmentName.Trim();
            query = query.Where(a => a.Doctor.Departments.Any(d => d.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var term = status.Trim();
            query = query.Where(a => a.Status.ToString().Contains(term));
        }

        if (from is not null)
        {
            query = query.Where(a => a.ScheduledAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(a => a.ScheduledAt <= to.Value);
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = appointments.Select(a => new AppointmentToolItem(
            a.Id,
            a.ScheduledAt.ToString("yyyy-MM-dd HH:mm"),
            $"{a.Patient.FirstName} {a.Patient.LastName}",
            $"{a.Doctor.FirstName} {a.Doctor.LastName}",
            a.Doctor.Specialty,
            a.Doctor.Departments.OrderBy(d => d.Name).Select(d => d.Name).FirstOrDefault(),
            a.Status.ToString(),
            a.Room)).ToList();

        return new("search_appointments", BuildCriteria((nameof(doctorName), doctorName), (nameof(patientName), patientName), (nameof(departmentName), departmentName), (nameof(status), status), (nameof(dateFrom), dateFrom), (nameof(dateTo), dateTo)), items.Count, items);
    }

    [Description("Search doctors by name, specialty, and department.")]
    public async Task<DataAssistantToolResult<DoctorToolItem>> SearchDoctorsAsync(
        string? doctorName = null,
        string? specialty = null,
        string? departmentName = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cappedLimit = CapLimit(limit);

        var query = _context.Doctors
            .AsNoTracking()
            .Include(d => d.Departments)
            .Include(d => d.Appointments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(doctorName))
        {
            var term = doctorName.Trim();
            query = query.Where(d => (d.FirstName + " " + d.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var term = specialty.Trim();
            query = query.Where(d => d.Specialty.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(departmentName))
        {
            var term = departmentName.Trim();
            query = query.Where(d => d.Departments.Any(dep => dep.Name.Contains(term)));
        }

        var doctors = await query
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = doctors.Select(d => new DoctorToolItem(
            d.Id,
            $"{d.FirstName} {d.LastName}",
            d.Specialty,
            d.Departments.OrderBy(dep => dep.Name).Select(dep => dep.Name).ToList(),
            d.Appointments.Count(a => a.ScheduledAt >= now))).ToList();

        return new("search_doctors", BuildCriteria((nameof(doctorName), doctorName), (nameof(specialty), specialty), (nameof(departmentName), departmentName)), items.Count, items);
    }

    [Description("Get department overview. Bed availability is not tracked in this app.")]
    public async Task<DataAssistantToolResult<DepartmentToolItem>> GetDepartmentOverviewAsync(
        string? departmentName = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var through = now.AddDays(7);
        var cappedLimit = CapLimit(limit);

        var query = _context.Departments
            .AsNoTracking()
            .Include(d => d.Doctors)
                .ThenInclude(doc => doc.Appointments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(departmentName))
        {
            var term = departmentName.Trim();
            query = query.Where(d => d.Name.Contains(term));
        }

        var departments = await query
            .OrderBy(d => d.Name)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = departments.Select(d => new DepartmentToolItem(
            d.Id,
            d.Name,
            d.Location,
            d.HeadOfDepartment,
            d.Doctors.Count,
            d.Doctors.SelectMany(doc => doc.Appointments).Count(a => a.ScheduledAt >= now && a.ScheduledAt <= through),
            "Bed availability is not represented in the current data model.")).ToList();

        return new("get_department_overview", BuildCriteria((nameof(departmentName), departmentName)), items.Count, items, "This app currently has no Beds entity or bed availability field.");
    }

    [Description("Search medical records by patient, diagnosis, and created date range. Free-text notes are not returned.")]
    public async Task<DataAssistantToolResult<MedicalRecordToolItem>> SearchMedicalRecordsAsync(
        string? patientName = null,
        string? diagnosis = null,
        string? createdFrom = null,
        string? createdTo = null,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Min(CapLimit(limit), 10);
        var from = ParseDate(createdFrom);
        var to = ParseDate(createdTo, endOfDay: true);

        var query = _context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var term = patientName.Trim();
            query = query.Where(r => (r.Patient.FirstName + " " + r.Patient.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(diagnosis))
        {
            var term = diagnosis.Trim();
            query = query.Where(r => r.Diagnosis.Contains(term));
        }

        if (from is not null)
        {
            query = query.Where(r => r.CreatedAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(r => r.CreatedAt <= to.Value);
        }

        var records = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = records.Select(r => new MedicalRecordToolItem(
            r.Id,
            r.CreatedAt.ToString("yyyy-MM-dd"),
            $"{r.Patient.FirstName} {r.Patient.LastName}",
            r.Diagnosis,
            r.Prescriptions.Count)).ToList();

        return new("search_medical_records", BuildCriteria((nameof(patientName), patientName), (nameof(diagnosis), diagnosis), (nameof(createdFrom), createdFrom), (nameof(createdTo), createdTo)), items.Count, items, "Free-text medical notes were intentionally omitted.");
    }

    [Description("Search prescriptions by patient, diagnosis, issuer, and issued date range.")]
    public async Task<DataAssistantToolResult<PrescriptionToolItem>> SearchPrescriptionsAsync(
        string? patientName = null,
        string? diagnosis = null,
        string? issuedBy = null,
        string? issuedFrom = null,
        string? issuedTo = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = CapLimit(limit);
        var from = ParseDate(issuedFrom);
        var to = ParseDate(issuedTo, endOfDay: true);

        var query = _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
                .ThenInclude(r => r.Patient)
            .Include(p => p.Medications)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var term = patientName.Trim();
            query = query.Where(p => (p.MedicalRecord.Patient.FirstName + " " + p.MedicalRecord.Patient.LastName).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(diagnosis))
        {
            var term = diagnosis.Trim();
            query = query.Where(p => p.MedicalRecord.Diagnosis.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(issuedBy))
        {
            var term = issuedBy.Trim();
            query = query.Where(p => p.IssuedBy.Contains(term));
        }

        if (from is not null)
        {
            query = query.Where(p => p.IssuedAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(p => p.IssuedAt <= to.Value);
        }

        var prescriptions = await query
            .OrderByDescending(p => p.IssuedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = prescriptions.Select(p => new PrescriptionToolItem(
            p.Id,
            p.IssuedAt.ToString("yyyy-MM-dd"),
            p.IssuedBy,
            $"{p.MedicalRecord.Patient.FirstName} {p.MedicalRecord.Patient.LastName}",
            p.MedicalRecord.Diagnosis,
            p.Medications.Count)).ToList();

        return new("search_prescriptions", BuildCriteria((nameof(patientName), patientName), (nameof(diagnosis), diagnosis), (nameof(issuedBy), issuedBy), (nameof(issuedFrom), issuedFrom), (nameof(issuedTo), issuedTo)), items.Count, items);
    }

    [Description("Search medications by name, dosage, and instructions.")]
    public async Task<DataAssistantToolResult<MedicationToolItem>> SearchMedicationsAsync(
        string? medicationName = null,
        string? dosage = null,
        string? instructions = null,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = CapLimit(limit);

        var query = _context.Medications
            .AsNoTracking()
            .Include(m => m.Prescription)
                .ThenInclude(p => p.MedicalRecord)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(medicationName))
        {
            var term = medicationName.Trim();
            query = query.Where(m => m.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(dosage))
        {
            var term = dosage.Trim();
            query = query.Where(m => m.Dosage.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(instructions))
        {
            var term = instructions.Trim();
            query = query.Where(m => m.Instructions != null && m.Instructions.Contains(term));
        }

        var medications = await query
            .OrderBy(m => m.Name)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = medications.Select(m => new MedicationToolItem(
            m.Id,
            m.Name,
            m.Dosage,
            m.Instructions,
            m.Prescription.IssuedBy,
            m.Prescription.MedicalRecord.Diagnosis)).ToList();

        return new("search_medications", BuildCriteria((nameof(medicationName), medicationName), (nameof(dosage), dosage), (nameof(instructions), instructions)), items.Count, items);
    }

    private static int CapLimit(int limit) => Math.Clamp(limit <= 0 ? DefaultLimit : limit, 1, MaxLimit);

    private static DateTime? ParseDate(string? value, bool endOfDay = false)
    {
        if (string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value, out var parsed))
        {
            return null;
        }

        var date = parsed.Date;
        return endOfDay ? date.AddDays(1).AddTicks(-1) : date;
    }

    private static int CalculateAge(DateTime dateOfBirth, DateTime now)
    {
        var age = now.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > now.Date.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static string BuildCriteria(params (string Name, object? Value)[] criteria)
    {
        var active = criteria
            .Where(c => c.Value is not null && !string.IsNullOrWhiteSpace(c.Value.ToString()))
            .Select(c => $"{c.Name}={c.Value}");

        var result = string.Join(", ", active);
        return string.IsNullOrWhiteSpace(result) ? "none" : result;
    }
}
