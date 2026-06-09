using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Tests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<int> CreatePatientAsync(AppDbContext context)
    {
        var patient = new Patient
        {
            FirstName = "John",
            LastName = "Tester",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = $"patient-{Guid.NewGuid():N}@test.local",
            PhoneNumber = "+385111111",
            Address = "Test Address"
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    public static async Task<int> CreateDepartmentAsync(AppDbContext context)
    {
        var department = new Department
        {
            Name = $"Department-{Guid.NewGuid():N}"[..20],
            Location = "Wing A"
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync();
        return department.Id;
    }

    public static async Task<int> CreateDoctorAsync(AppDbContext context, params int[] departmentIds)
    {
        var doctor = new Doctor
        {
            FirstName = "Greg",
            LastName = $"House{Guid.NewGuid():N}"[..6],
            Gender = Gender.Male,
            Specialty = "Diagnostics",
            Email = $"doctor-{Guid.NewGuid():N}@test.local",
            PhoneNumber = "+385222222"
        };

        if (departmentIds.Length > 0)
        {
            doctor.Departments = await context.Departments.Where(d => departmentIds.Contains(d.Id)).ToListAsync();
        }

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();
        return doctor.Id;
    }

    public static async Task<int> CreateMedicalRecordAsync(AppDbContext context, int patientId)
    {
        var record = new MedicalRecord
        {
            PatientId = patientId,
            CreatedAt = DateTime.UtcNow,
            Diagnosis = "Test diagnosis",
            Notes = "Test notes"
        };

        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();
        return record.Id;
    }

    public static async Task<int> CreatePrescriptionAsync(AppDbContext context, int medicalRecordId)
    {
        var prescription = new Prescription
        {
            MedicalRecordId = medicalRecordId,
            IssuedAt = DateTime.UtcNow,
            IssuedBy = "Dr. Seeder"
        };

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
        return prescription.Id;
    }

    public static async Task<int> CreateMedicationAsync(AppDbContext context, int prescriptionId)
    {
        var medication = new Medication
        {
            Name = "Ibuprofen",
            Dosage = "200mg",
            Instructions = "After meal",
            PrescriptionId = prescriptionId
        };

        context.Medications.Add(medication);
        await context.SaveChangesAsync();
        return medication.Id;
    }

    public static async Task<int> CreateAppointmentAsync(AppDbContext context, int patientId, int doctorId)
    {
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status = AppointmentStatus.CheckedIn,
            Room = "A12",
            Notes = "Initial check"
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }
}
