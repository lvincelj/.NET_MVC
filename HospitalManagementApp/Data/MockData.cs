using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

// Central static seed — all repositories pull from here so objects stay shared
// and navigation properties (Appointments, MedicalRecords, etc.) are populated.
public static class MockData
{
    // ── Departments ──────────────────────────────────────────────────────────
    public static readonly Department DeptCardiology = new Department
    {
        Id = 1,
        Name = "Cardiology",
        Location = "Building A - Floor 2",
        PhoneNumber = "+1-555-0101",
        HeadOfDepartment = "Dr. Alice Heart"
    };

    public static readonly Department DeptNeurology = new Department
    {
        Id = 2,
        Name = "Neurology",
        Location = "Building B - Floor 3",
        PhoneNumber = "+1-555-0202",
        HeadOfDepartment = "Dr. Brian Nerve"
    };

    public static readonly Department DeptGeneralMed = new Department
    {
        Id = 3,
        Name = "General Medicine",
        Location = "Building A - Floor 1",
        PhoneNumber = "+1-555-0303",
        HeadOfDepartment = "Dr. Carol Well"
    };

    // ── Doctors ───────────────────────────────────────────────────────────────
    public static readonly Doctor Doc1 = new Doctor
    {
        Id = 1,
        FirstName = "Alice",
        LastName = "Heart",
        Gender = Gender.Female,
        Specialty = "Cardiologist",
        Email = "alice.heart@hospital.com",
        PhoneNumber = "+1-555-1001"
    };

    public static readonly Doctor Doc2 = new Doctor
    {
        Id = 2,
        FirstName = "Brian",
        LastName = "Nerve",
        Gender = Gender.Male,
        Specialty = "Neurologist",
        Email = "brian.nerve@hospital.com",
        PhoneNumber = "+1-555-1002"
    };

    public static readonly Doctor Doc3 = new Doctor
    {
        Id = 3,
        FirstName = "Carol",
        LastName = "Well",
        Gender = Gender.Female,
        Specialty = "General Practitioner",
        Email = "carol.well@hospital.com",
        PhoneNumber = "+1-555-1003"
    };

    // ── Patients ──────────────────────────────────────────────────────────────
    public static readonly Patient Pat1 = new Patient
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe",
        Gender = Gender.Male,
        DateOfBirth = new DateTime(1980, 5, 12),
        Email = "john.doe@email.com",
        PhoneNumber = "+1-555-2001",
        Address = "12 Oak Street, Springfield"
    };

    public static readonly Patient Pat2 = new Patient
    {
        Id = 2,
        FirstName = "Maria",
        LastName = "Smith",
        Gender = Gender.Female,
        DateOfBirth = new DateTime(1990, 8, 23),
        Email = "maria.smith@email.com",
        PhoneNumber = "+1-555-2002",
        Address = "45 Elm Avenue, Shelbyville"
    };

    public static readonly Patient Pat3 = new Patient
    {
        Id = 3,
        FirstName = "Luka",
        LastName = "Vincelj",
        Gender = Gender.Male,
        DateOfBirth = new DateTime(1995, 3, 3),
        Email = "luka.vincelj@email.com",
        PhoneNumber = "+1-555-2003",
        Address = "7 Maple Road, Capital City"
    };

    // ── Medications ───────────────────────────────────────────────────────────
    public static readonly Medication Med1 = new Medication
    {
        Id = 1,
        Name = "Lisinopril",
        Dosage = "10mg",
        Instructions = "Once daily for blood pressure",
        PrescriptionId = 1
    };

    public static readonly Medication Med2 = new Medication
    {
        Id = 2,
        Name = "Sumatriptan",
        Dosage = "50mg",
        Instructions = "Take at onset of migraine",
        PrescriptionId = 2
    };

    public static readonly Medication Med3 = new Medication
    {
        Id = 3,
        Name = "Ibuprofen",
        Dosage = "400mg",
        Instructions = "Every 8 hours with food",
        PrescriptionId = 3
    };

    // ── Prescriptions ─────────────────────────────────────────────────────────
    public static readonly Prescription Presc1 = new Prescription
    {
        Id = 1,
        MedicalRecordId = 1,
        IssuedBy = "Heart",
        IssuedAt = new DateTime(2026, 1, 10)
    };

    public static readonly Prescription Presc2 = new Prescription
    {
        Id = 2,
        MedicalRecordId = 2,
        IssuedBy = "Nerve",
        IssuedAt = new DateTime(2026, 2, 5)
    };

    public static readonly Prescription Presc3 = new Prescription
    {
        Id = 3,
        MedicalRecordId = 3,
        IssuedBy = "Well",
        IssuedAt = new DateTime(2026, 3, 18)
    };

    // ── Medical Records ────────────────────────────────────────────────────────
    public static readonly MedicalRecord Mr1 = new MedicalRecord
    {
        Id = 1,
        PatientId = 1,
        CreatedAt = new DateTime(2026, 1, 10),
        Diagnosis = "Hypertension",
        Notes = "Patient requires BP monitoring.",
        Patient = Pat1
    };

    public static readonly MedicalRecord Mr2 = new MedicalRecord
    {
        Id = 2,
        PatientId = 2,
        CreatedAt = new DateTime(2026, 2, 5),
        Diagnosis = "Chronic Migraine",
        Notes = "Avoid bright lights and loud noises.",
        Patient = Pat2
    };

    public static readonly MedicalRecord Mr3 = new MedicalRecord
    {
        Id = 3,
        PatientId = 3,
        CreatedAt = new DateTime(2026, 3, 18),
        Diagnosis = "Lower Back Strain",
        Notes = "Physical therapy referred.",
        Patient = Pat3
    };

    // ── Appointments ───────────────────────────────────────────────────────────
    public static readonly Appointment Appt1 = new Appointment
    {
        Id = 1,
        PatientId = 1,
        DoctorId = 1,
        ScheduledAt = DateTime.Today.AddDays(1),
        Status = AppointmentStatus.Scheduled,
        Room = "A201",
        Notes = "Regular BP check-up.",
        Patient = Pat1,
        Doctor = Doc1
    };

    public static readonly Appointment Appt2 = new Appointment
    {
        Id = 2,
        PatientId = 2,
        DoctorId = 2,
        ScheduledAt = DateTime.Today.AddDays(2),
        Status = AppointmentStatus.Scheduled,
        Room = "B310",
        Notes = "Follow-up after migraine episode.",
        Patient = Pat2,
        Doctor = Doc2
    };

    public static readonly Appointment Appt3 = new Appointment
    {
        Id = 3,
        PatientId = 3,
        DoctorId = 3,
        ScheduledAt = DateTime.Today.AddDays(3),
        Status = AppointmentStatus.Scheduled,
        Room = "A105",
        Notes = "Physical therapy referral review.",
        Patient = Pat3,
        Doctor = Doc3
    };

    // ── Wire up navigation properties ─────────────────────────────────────────
    static MockData()
    {
        // Departments <-> Doctors
        Doc1.Departments.Add(DeptCardiology);
        DeptCardiology.Doctors.Add(Doc1);

        Doc2.Departments.Add(DeptNeurology);
        DeptNeurology.Doctors.Add(Doc2);

        Doc3.Departments.Add(DeptGeneralMed);
        DeptGeneralMed.Doctors.Add(Doc3);

        // Prescriptions <-> Medications
        Med1.Prescription = Presc1;
        Med2.Prescription = Presc2;
        Med3.Prescription = Presc3;

        Presc1.Medications.Add(Med1);
        Presc2.Medications.Add(Med2);
        Presc3.Medications.Add(Med3);

        // Medical Records <-> Prescriptions
        Mr1.Prescriptions.Add(Presc1);
        Mr2.Prescriptions.Add(Presc2);
        Mr3.Prescriptions.Add(Presc3);

        Presc1.MedicalRecord = Mr1;
        Presc2.MedicalRecord = Mr2;
        Presc3.MedicalRecord = Mr3;

        // Patients <-> Medical Records
        Pat1.MedicalRecords.Add(Mr1);
        Pat2.MedicalRecords.Add(Mr2);
        Pat3.MedicalRecords.Add(Mr3);

        // Patients <-> Appointments
        Pat1.Appointments.Add(Appt1);
        Pat2.Appointments.Add(Appt2);
        Pat3.Appointments.Add(Appt3);

        // Doctors <-> Appointments
        Doc1.Appointments.Add(Appt1);
        Doc2.Appointments.Add(Appt2);
        Doc3.Appointments.Add(Appt3);
    }
}
