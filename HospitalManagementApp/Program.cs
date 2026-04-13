using System;
using System.Collections.Generic;
using HospitalManagementApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ============================================================
// 1. DEPARTMENTS (The 3 main core objects)
// ============================================================
var deptCardiology = new Department
{
    Id = 1,
    Name = "Cardiology",
    Location = "Building A - Floor 2",
    PhoneNumber = "+1-555-0101",
    HeadOfDepartment = "Dr. Alice Heart"
};

var deptNeurology = new Department
{
    Id = 2,
    Name = "Neurology",
    Location = "Building B - Floor 3",
    PhoneNumber = "+1-555-0202",
    HeadOfDepartment = "Dr. Brian Nerve"
};

var deptGeneralMed = new Department
{
    Id = 3,
    Name = "General Medicine",
    Location = "Building A - Floor 1",
    PhoneNumber = "+1-555-0303",
    HeadOfDepartment = "Dr. Carol Well"
};

// ============================================================
// 2. DOCTORS (Assigned to their respective departments)
// ============================================================
var doc1 = new Doctor { Id = 1, FirstName = "Alice", LastName = "Heart", Specialty = "Cardiologist", Email = "alice.heart@hospital.com" };
var doc2 = new Doctor { Id = 2, FirstName = "Brian", LastName = "Nerve", Specialty = "Neurologist", Email = "brian.nerve@hospital.com" };
var doc3 = new Doctor { Id = 3, FirstName = "Carol", LastName = "Well", Specialty = "General Practitioner", Email = "carol.well@hospital.com" };

// Establishing Many-to-Many relationships
doc1.Departments.Add(deptCardiology);
deptCardiology.Doctors.Add(doc1);

doc2.Departments.Add(deptNeurology);
deptNeurology.Doctors.Add(doc2);

doc3.Departments.Add(deptGeneralMed);
deptGeneralMed.Doctors.Add(doc3);

// ============================================================
// 3. PATIENTS
// ============================================================
var pat1 = new Patient { Id = 1, FirstName = "John", LastName = "Doe", Gender = Gender.Male, DateOfBirth = new DateTime(1980, 5, 12) };
var pat2 = new Patient { Id = 2, FirstName = "Maria", LastName = "Smith", Gender = Gender.Female, DateOfBirth = new DateTime(1990, 8, 23) };
var pat3 = new Patient { Id = 3, FirstName = "Luka", LastName = "Vincelj", Gender = Gender.Male, DateOfBirth = new DateTime(1995, 3, 3) };

// ============================================================
// 4. MEDICAL RECORDS & PRESCRIPTIONS (Linking medical logic)
// ============================================================

// --- Case 1: Cardiology (Heart Issues) ---
var med1 = new Medication { Id = 1, Name = "Lisinopril", Dosage = "10mg", Instructions = "Once daily for blood pressure" };
var presc1 = new Prescription { Id = 1, IssuedBy = doc1.LastName, IssuedAt = DateTime.Now };
presc1.Medications.Add(med1);

var mr1 = new MedicalRecord { Id = 1, PatientId = 1, Diagnosis = "Hypertension", Notes = "Patient requires BP monitoring.", Patient = pat1 };
mr1.Prescriptions.Add(presc1);
pat1.MedicalRecords.Add(mr1);

// --- Case 2: Neurology (Migraines) ---
var med2 = new Medication { Id = 2, Name = "Sumatriptan", Dosage = "50mg", Instructions = "Take at onset of migraine" };
var presc2 = new Prescription { Id = 2, IssuedBy = doc2.LastName, IssuedAt = DateTime.Now };
presc2.Medications.Add(med2);

var mr2 = new MedicalRecord { Id = 2, PatientId = 2, Diagnosis = "Chronic Migraine", Notes = "Avoid bright lights and loud noises.", Patient = pat2 };
mr2.Prescriptions.Add(presc2);
pat2.MedicalRecords.Add(mr2);

// --- Case 3: General Medicine (Physical Injury) ---
var med3 = new Medication { Id = 3, Name = "Ibuprofen", Dosage = "400mg", Instructions = "Every 8 hours with food" };
var presc3 = new Prescription { Id = 3, IssuedBy = doc3.LastName, IssuedAt = DateTime.Now };
presc3.Medications.Add(med3);

var mr3 = new MedicalRecord { Id = 3, PatientId = 3, Diagnosis = "Lower Back Strain", Notes = "Physical therapy referred.", Patient = pat3 };
mr3.Prescriptions.Add(presc3);
pat3.MedicalRecords.Add(mr3);

// ============================================================
// 5. APPOINTMENTS (Scheduling the visits)
// ============================================================
var appointments = new List<Appointment>
{
    new Appointment { Id = 1, Patient = pat1, Doctor = doc1, ScheduledAt = DateTime.Now.AddDays(1), Status = AppointmentStatus.Scheduled, Room = "A201" },
    new Appointment { Id = 2, Patient = pat2, Doctor = doc2, ScheduledAt = DateTime.Now.AddDays(2), Status = AppointmentStatus.Scheduled, Room = "B310" },
    new Appointment { Id = 3, Patient = pat3, Doctor = doc3, ScheduledAt = DateTime.Now.AddDays(3), Status = AppointmentStatus.Scheduled, Room = "A105" }
};

// ============================================================
// CONSOLE OUTPUT (Verification)
// ============================================================
Console.WriteLine("=== HOSPITAL MANAGEMENT SYSTEM - DATA SUMMARY ===\n");

var allPatients = new List<Patient> { pat1, pat2, pat3 };
foreach (var patient in allPatients)
{
    Console.WriteLine($"PATIENT: {patient.FirstName} {patient.LastName}");
    foreach (var record in patient.MedicalRecords)
    {
        Console.WriteLine($"  [Diagnosis]: {record.Diagnosis}");
        foreach (var p in record.Prescriptions)
        {
            Console.WriteLine($"  [Prescribed By]: Dr. {p.IssuedBy}");
            foreach (var m in p.Medications)
            {
                Console.WriteLine($"    -> Medication: {m.Name} ({m.Dosage}) - {m.Instructions}");
            }
        }
    }
    Console.WriteLine(new string('-', 50));
}

// ============================================================
// 6. LINQ QUERIES
// ============================================================

// Query 1: Female patients with at least one medical record, ordered by last name
var femalePatients = allPatients
    .Where(p => p.Gender == Gender.Female && p.MedicalRecords.Any())
    .OrderBy(p => p.LastName)
    .ThenBy(p => p.FirstName)
    .ToList();
Console.WriteLine("\n=== LINQ 1: Female Patients With Records (Sorted) ===");
foreach (var p in femalePatients)
{
    Console.WriteLine($"  {p.FirstName} {p.LastName} - Records: {p.MedicalRecords.Count}");
}

// Query 2: Sort appointments by date, then doctor, and project to a compact view model
var sortedAppointments = appointments
    .OrderBy(a => a.ScheduledAt)
    .ThenBy(a => a.Doctor.LastName)
    .Select(a => new
    {
        PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
        DoctorLastName = a.Doctor.LastName,
        Date = a.ScheduledAt,
        a.Room
    })
    .ToList();
Console.WriteLine("\n=== LINQ 2: Appointments Sorted by Date + Doctor ===");
foreach (var a in sortedAppointments)
{
    Console.WriteLine($"  {a.PatientFullName} with Dr. {a.DoctorLastName} on {a.Date:yyyy-MM-dd} in Room {a.Room}");
}

// Query 3: Get patient full names with age and order by age descending
var patientNames = allPatients
    .Select(p => new
    {
        FullName = p.FirstName + " " + p.LastName,
        Age = DateTime.Today.Year - p.DateOfBirth.Year - (DateTime.Today.DayOfYear < p.DateOfBirth.DayOfYear ? 1 : 0)
    })
    .OrderByDescending(p => p.Age)
    .ToList();
Console.WriteLine("\n=== LINQ 3: Patient Names With Age (Oldest First) ===");
foreach (var p in patientNames)
{
    Console.WriteLine($"  {p.FullName} - {p.Age} years");
}

// Query 4: Patients born before 1991, including total number of prescribed medications
var olderPatients = allPatients
    .Where(p => p.DateOfBirth.Year < 1991)
    .Select(p => new
    {
        Patient = p,
        MedicationCount = p.MedicalRecords
            .SelectMany(r => r.Prescriptions)
            .SelectMany(pr => pr.Medications)
            .Count()
    })
    .OrderByDescending(x => x.MedicationCount)
    .ToList();
Console.WriteLine("\n=== LINQ 4: Patients Born Before 1991 + Medication Count ===");
foreach (var p in olderPatients)
{
    Console.WriteLine($"  {p.Patient.FirstName} {p.Patient.LastName} (born {p.Patient.DateOfBirth:yyyy-MM-dd}) - Medications: {p.MedicationCount}");
}

// Query 5: Doctors in Building A with matching department names
var allDoctors = new List<Doctor> { doc1, doc2, doc3 };
var doctorsInBuildingA = allDoctors
    .Where(d => d.Departments.Any(dept => dept.Location.Contains("Building A")))
    .Select(d => new
    {
        Doctor = d,
        BuildingADepartments = d.Departments
            .Where(dept => dept.Location.Contains("Building A"))
            .Select(dept => dept.Name)
            .ToList()
    })
    .OrderBy(x => x.Doctor.LastName)
    .ToList();
Console.WriteLine("\n=== LINQ 5: Doctors in Building A + Departments ===");
foreach (var d in doctorsInBuildingA)
{
    Console.WriteLine($"  Dr. {d.Doctor.FirstName} {d.Doctor.LastName} ({d.Doctor.Specialty}) - {string.Join(", ", d.BuildingADepartments)}");
}

// Query 6: Find the next scheduled appointment in Room A201 (from now onward)
var firstAppointmentInA201 = appointments
    .Where(a => a.Room == "A201" && a.Status == AppointmentStatus.Scheduled && a.ScheduledAt >= DateTime.Now)
    .OrderBy(a => a.ScheduledAt)
    .FirstOrDefault();
Console.WriteLine("\n=== LINQ 6: Next Scheduled Appointment In Room A201 ===");
if (firstAppointmentInA201 != null)
{
    Console.WriteLine($"  {firstAppointmentInA201.Patient.FirstName} {firstAppointmentInA201.Patient.LastName} with Dr. {firstAppointmentInA201.Doctor.LastName} on {firstAppointmentInA201.ScheduledAt:yyyy-MM-dd HH:mm}");
}
else
{
    Console.WriteLine("  No upcoming scheduled appointment found in Room A201.");
}

var app = builder.Build();

// Standard ASP.NET Core Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
