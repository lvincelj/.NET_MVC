using System;
using System.Collections.Generic;
using HospitalManagementApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── Departments ──
var dept1 = new Department 
{ 
    Id = 1, 
    Name = "Cardiology", 
    Location = "Building A - Floor 2", 
    PhoneNumber = "+1-555-0101", 
    HeadOfDepartment = "Dr. Alice Heart" 
};

var dept2 = new Department 
{ 
    Id = 2, 
    Name = "Neurology", 
    Location = "Building B - Floor 3", 
    PhoneNumber = "+1-555-0202",
    HeadOfDepartment = "Dr. Brian Nerve" 
};

var dept3 = new Department
 { 
    Id = 3, 
    Name = "General Medicine", 
    Location = "Building A - Floor 1", 
    PhoneNumber = "+1-555-0303", 
    HeadOfDepartment = "Dr. Carol Well" 
};

// ── Doctors ──
var doc1 = new Doctor
{
    Id = 1,
    FirstName = "Alice",
    LastName = "Heart",
    Gender = Gender.Female,
    Specialty = "Cardiologist",
    Email = "alice.heart@hospital.com",
    PhoneNumber = "+1-555-1101"
};

var doc2 = new Doctor
{
    Id = 2,
    FirstName = "Brian",
    LastName = "Nerve",
    Gender = Gender.Male,
    Specialty = "Neurologist",
    Email = "brian.nerve@hospital.com",
    PhoneNumber = "+1-555-1202"
};

var doc3 = new Doctor
{
    Id = 3,
    FirstName = "Carol",
    LastName = "Well",
    Gender = Gender.Female,
    Specialty = "General Practitioner",
    Email = "carol.well@hospital.com",
    PhoneNumber = "+1-555-1303"
};

// assign departments to doctors (many-to-many)
doc1.Departments.Add(dept1);
doc1.Departments.Add(dept3);
dept1.Doctors.Add(doc1);
dept3.Doctors.Add(doc1);

doc2.Departments.Add(dept2);
dept2.Doctors.Add(doc2);

doc3.Departments.Add(dept3);
dept3.Doctors.Add(doc3);

// ── Patients ──
var pat1 = new Patient
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Gender = Gender.Male,
    DateOfBirth = new DateTime(1980, 5, 12),
    Email = "john.doe@example.com",
    PhoneNumber = "+1-555-2101",
    Address = "123 Maple St"
};

var pat2 = new Patient
{
    Id = 2,
    FirstName = "Maria",
    LastName = "Smith",
    Gender = Gender.Female,
    DateOfBirth = new DateTime(1990, 8, 23),
    Email = "maria.smith@example.com",
    PhoneNumber = "+1-555-2202",
    Address = "45 Oak Ave"
};

var pat3 = new Patient
{
    Id = 3,
    FirstName = "Luka",
    LastName = "Vincelj",
    Gender = Gender.Male,
    DateOfBirth = new DateTime(1995, 3, 3),
    Email = "luka.vincelj@example.com",
    PhoneNumber = "+1-555-2303",
    Address = "78 Pine Rd"
};

// ── Medications ──
var med1 = new Medication 
{ 
    Id = 1, 
    Name = "Lisinopril", 
    Dosage = "10mg", 
    Instructions = "Once daily in the morning", 
    PrescriptionId = 1 
};

var med2 = new Medication 
{ 
    Id = 2, 
    Name = "Cetirizine", 
    Dosage = "10mg", 
    Instructions = "Once daily at night", 
    PrescriptionId = 2 
};

var med3 = new Medication 
{ 
    Id = 3, 
    Name = "Ibuprofen", 
    Dosage = "400mg", 
    Instructions = "Every 6 hours as needed", 
    PrescriptionId = 3 
};


// ── Prescriptions ──
var presc1 = new Prescription 
{ 
    Id = 1, 
    MedicalRecordId = 1, 
    IssuedAt = new DateTime(2026, 1, 10), 
    IssuedBy = "Dr. Alice Heart" 
};
presc1.Medications.Add(med1);

var presc2 = new Prescription 
{ 
    Id = 2, 
    MedicalRecordId = 2, 
    IssuedAt = new DateTime(2026, 2, 5), 
    IssuedBy = "Dr. Brian Nerve" 
};
presc2.Medications.Add(med2);

var presc3 = new Prescription 
{ 
    Id = 3, 
    MedicalRecordId = 3, 
    IssuedAt = new DateTime(2026, 3, 1), 
    IssuedBy = "Dr. Carol Well" 
};
presc3.Medications.Add(med3);


// ── Medical Records ──
var mr1 = new MedicalRecord 
{ 
    Id = 1, 
    PatientId = 1, 
    CreatedAt = new DateTime(2026, 1, 9), 
    Diagnosis = "Hypertension", 
    Notes = "Prescribed lifestyle changes and blood pressure monitoring." 
};
mr1.Prescriptions.Add(presc1);
mr1.Patient = pat1;
pat1.MedicalRecords.Add(mr1);

var mr2 = new MedicalRecord 
{ 
    Id = 2, 
    PatientId = 2, 
    CreatedAt = new DateTime(2026, 2, 4), 
    Diagnosis = "Seasonal Allergy", 
    Notes = "Prescribed antihistamine for pollen allergy." 
};
mr2.Prescriptions.Add(presc2);
mr2.Patient = pat2;
pat2.MedicalRecords.Add(mr2);

var mr3 = new MedicalRecord 
{ 
    Id = 3, 
    PatientId = 3, 
    CreatedAt = new DateTime(2026, 3, 1), 
    Diagnosis = "Lower back pain", 
    Notes = "Prescribed anti-inflammatory medication and physical therapy." 
};
mr3.Prescriptions.Add(presc3);
mr3.Patient = pat3;
pat3.MedicalRecords.Add(mr3);

// ── Appointments (each doctor has at least 1, 9 total) ──
var appt1 = new Appointment { Id = 1, PatientId = 1, DoctorId = 1, ScheduledAt = new DateTime(2026, 4, 1, 9, 0, 0), Status = AppointmentStatus.Scheduled, Room = "A201", Notes = "Follow-up on blood pressure.", Patient = pat1, Doctor = doc1 };
var appt2 = new Appointment { Id = 2, PatientId = 2, DoctorId = 1, ScheduledAt = new DateTime(2026, 4, 1, 10, 0, 0), Status = AppointmentStatus.Scheduled, Room = "A201", Notes = "Cardiac screening.", Patient = pat2, Doctor = doc1 };
var appt3 = new Appointment { Id = 3, PatientId = 3, DoctorId = 1, ScheduledAt = new DateTime(2026, 4, 2, 9, 0, 0), Status = AppointmentStatus.Scheduled, Room = "A202", Notes = "ECG checkup.", Patient = pat3, Doctor = doc1 };

var appt4 = new Appointment { Id = 4, PatientId = 1, DoctorId = 2, ScheduledAt = new DateTime(2026, 4, 3, 11, 0, 0), Status = AppointmentStatus.Scheduled, Room = "B310", Notes = "Headache evaluation.", Patient = pat1, Doctor = doc2 };
var appt5 = new Appointment { Id = 5, PatientId = 2, DoctorId = 2, ScheduledAt = new DateTime(2026, 4, 4, 14, 0, 0), Status = AppointmentStatus.Completed, Room = "B310", Notes = "Neurology consult for migraines.", Patient = pat2, Doctor = doc2 };
var appt6 = new Appointment { Id = 6, PatientId = 3, DoctorId = 2, ScheduledAt = new DateTime(2026, 4, 5, 9, 30, 0), Status = AppointmentStatus.Cancelled, Room = "B311", Notes = "Nerve conduction study.", Patient = pat3, Doctor = doc2 };

var appt7 = new Appointment { Id = 7, PatientId = 1, DoctorId = 3, ScheduledAt = new DateTime(2026, 4, 6, 8, 0, 0), Status = AppointmentStatus.Completed, Room = "A105", Notes = "Annual physical.", Patient = pat1, Doctor = doc3 };
var appt8 = new Appointment { Id = 8, PatientId = 2, DoctorId = 3, ScheduledAt = new DateTime(2026, 4, 7, 10, 30, 0), Status = AppointmentStatus.Scheduled, Room = "A105", Notes = "General checkup.", Patient = pat2, Doctor = doc3 };
var appt9 = new Appointment { Id = 9, PatientId = 3, DoctorId = 3, ScheduledAt = new DateTime(2026, 4, 8, 15, 0, 0), Status = AppointmentStatus.CheckedIn, Room = "A106", Notes = "Back pain follow-up.", Patient = pat3, Doctor = doc3 };

// link appointments to patients and doctors
pat1.Appointments.AddRange(new[] { appt1, appt4, appt7 });
pat2.Appointments.AddRange(new[] { appt2, appt5, appt8 });
pat3.Appointments.AddRange(new[] { appt3, appt6, appt9 });

doc1.Appointments.AddRange(new[] { appt1, appt2, appt3 });
doc2.Appointments.AddRange(new[] { appt4, appt5, appt6 });
doc3.Appointments.AddRange(new[] { appt7, appt8, appt9 });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
