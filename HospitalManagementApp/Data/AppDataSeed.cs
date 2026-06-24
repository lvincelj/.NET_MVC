using Microsoft.EntityFrameworkCore;
using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public static class AppDataSeed
{
    private const string DemoMarker = "[Expanded Demo]";

    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await context.Patients.AnyAsync() && !await context.Doctors.AnyAsync())
        {
            context.AddRange(
                MockData.DeptCardiology,
                MockData.DeptNeurology,
                MockData.DeptGeneralMed,
                MockData.Doc1,
                MockData.Doc2,
                MockData.Doc3,
                MockData.Pat1,
                MockData.Pat2,
                MockData.Pat3,
                MockData.Mr1,
                MockData.Mr2,
                MockData.Mr3,
                MockData.Presc1,
                MockData.Presc2,
                MockData.Presc3,
                MockData.Med1,
                MockData.Med2,
                MockData.Med3,
                MockData.Appt1,
                MockData.Appt2,
                MockData.Appt3);

            await context.SaveChangesAsync();
        }

        await SeedExpandedDemoDataAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExpandedDemoDataAsync(AppDbContext context)
    {
        var cardiology = await EnsureDepartmentAsync(context, "Cardiology", "Building A - Floor 2", "+1-555-0101", "Dr. Alice Heart");
        var neurology = await EnsureDepartmentAsync(context, "Neurology", "Building B - Floor 3", "+1-555-0202", "Dr. Brian Nerve");
        var generalMedicine = await EnsureDepartmentAsync(context, "General Medicine", "Building A - Floor 1", "+1-555-0303", "Dr. Carol Well");
        var pediatrics = await EnsureDepartmentAsync(context, "Pediatrics", "Building C - Floor 1", "+1-555-0404", "Dr. Mila Novak");
        var orthopedics = await EnsureDepartmentAsync(context, "Orthopedics", "Building D - Floor 2", "+1-555-0505", "Dr. Ivan Horvat");
        var radiology = await EnsureDepartmentAsync(context, "Radiology", "Building B - Floor 1", "+1-555-0606", "Dr. Nina Kovac");
        var emergency = await EnsureDepartmentAsync(context, "Emergency Medicine", "Building E - Ground Floor", "+1-555-0707", "Dr. Omar Hadzic");

        var doctors = new[]
        {
            await EnsureDoctorAsync(context, "Jelena", "Care", Gender.Female, "Cardiologist", "jelena.care@hospital.com", "+1-555-1101", cardiology),
            await EnsureDoctorAsync(context, "Mila", "Novak", Gender.Female, "Pediatrician", "mila.novak@hospital.com", "+1-555-1102", pediatrics),
            await EnsureDoctorAsync(context, "Ivan", "Horvat", Gender.Male, "Orthopedic Surgeon", "ivan.horvat@hospital.com", "+1-555-1103", orthopedics),
            await EnsureDoctorAsync(context, "Nina", "Kovac", Gender.Female, "Radiologist", "nina.kovac@hospital.com", "+1-555-1104", radiology),
            await EnsureDoctorAsync(context, "Omar", "Hadzic", Gender.Male, "Emergency Physician", "omar.hadzic@hospital.com", "+1-555-1105", emergency),
            await EnsureDoctorAsync(context, "Petra", "Maric", Gender.Female, "Internist", "petra.maric@hospital.com", "+1-555-1106", generalMedicine),
            await EnsureDoctorAsync(context, "Marko", "Babic", Gender.Male, "Neurologist", "marko.babic@hospital.com", "+1-555-1107", neurology)
        };

        var patients = new[]
        {
            await EnsurePatientAsync(context, "Amina", "Hadzic", Gender.Female, new DateTime(1952, 4, 18), "amina.hadzic@example.com", "+1-555-2101", "18 River Walk, Springfield"),
            await EnsurePatientAsync(context, "Mateo", "Knezevic", Gender.Male, new DateTime(2016, 9, 2), "mateo.knezevic@example.com", "+1-555-2102", "9 Pine Lane, Shelbyville"),
            await EnsurePatientAsync(context, "Sara", "Basic", Gender.Female, new DateTime(1988, 11, 14), "sara.basic@example.com", "+1-555-2103", "31 Cedar Court, Capital City"),
            await EnsurePatientAsync(context, "Noah", "Johnson", Gender.Male, new DateTime(1974, 2, 7), "noah.johnson@example.com", "+1-555-2104", "84 Lake Drive, Springfield"),
            await EnsurePatientAsync(context, "Elena", "Garcia", Gender.Female, new DateTime(1949, 7, 30), "elena.garcia@example.com", "+1-555-2105", "22 Hill Street, Ogdenville"),
            await EnsurePatientAsync(context, "Filip", "Kovac", Gender.Male, new DateTime(2001, 12, 5), "filip.kovac@example.com", "+1-555-2106", "6 Market Road, North Haverbrook"),
            await EnsurePatientAsync(context, "Lejla", "Dedic", Gender.Female, new DateTime(1963, 5, 21), "lejla.dedic@example.com", "+1-555-2107", "103 Birch Avenue, Springfield"),
            await EnsurePatientAsync(context, "David", "Miller", Gender.Male, new DateTime(1996, 1, 12), "david.miller@example.com", "+1-555-2108", "44 West End, Shelbyville"),
            await EnsurePatientAsync(context, "Iva", "Peric", Gender.Female, new DateTime(2009, 3, 27), "iva.peric@example.com", "+1-555-2109", "15 Garden View, Capital City"),
            await EnsurePatientAsync(context, "Tomislav", "Juric", Gender.Male, new DateTime(1958, 10, 9), "tomislav.juric@example.com", "+1-555-2110", "71 Station Street, Ogdenville"),
            await EnsurePatientAsync(context, "Maja", "Sokol", Gender.Female, new DateTime(1982, 6, 16), "maja.sokol@example.com", "+1-555-2111", "27 Willow Square, Springfield"),
            await EnsurePatientAsync(context, "Adnan", "Sehic", Gender.Male, new DateTime(1970, 8, 3), "adnan.sehic@example.com", "+1-555-2112", "52 Bridge Road, Shelbyville")
        };

        await context.SaveChangesAsync();

        await EnsureRecordPlanAsync(context, patients[0], doctors[0], "Atrial fibrillation follow-up", "Stable rhythm today. Continue anticoagulation monitoring.", "Warfarin", "5mg", "Take once daily in the evening and monitor INR weekly.", DateTime.Today.AddDays(1), AppointmentStatus.Scheduled, "A204");
        await EnsureRecordPlanAsync(context, patients[1], doctors[1], "Seasonal asthma review", "Mild wheezing after exercise. Parent educated about inhaler use.", "Salbutamol inhaler", "100mcg", "Two puffs as needed for wheezing, maximum every 4 hours.", DateTime.Today.AddDays(2), AppointmentStatus.Scheduled, "C103");
        await EnsureRecordPlanAsync(context, patients[2], doctors[5], "Iron deficiency anemia", "Fatigue and low ferritin. Dietary counseling provided.", "Ferrous sulfate", "325mg", "Take every morning with vitamin C; avoid taking with dairy.", DateTime.Today.AddDays(4), AppointmentStatus.Scheduled, "A112");
        await EnsureRecordPlanAsync(context, patients[3], doctors[2], "Knee osteoarthritis flare", "Pain after prolonged walking. X-ray recommended if symptoms persist.", "Naproxen", "250mg", "Take twice daily with food for up to 7 days.", DateTime.Today.AddDays(5), AppointmentStatus.Scheduled, "D211");
        await EnsureRecordPlanAsync(context, patients[4], doctors[0], "Congestive heart failure monitoring", "Mild ankle swelling. Weight tracking reviewed.", "Furosemide", "20mg", "Take every morning and report sudden weight gain.", DateTime.Today.AddDays(7), AppointmentStatus.Scheduled, "A202");
        await EnsureRecordPlanAsync(context, patients[5], doctors[3], "Chest imaging review", "Follow-up after minor workplace injury. No acute findings reported.", "Paracetamol", "500mg", "Take every 6 hours as needed for pain.", DateTime.Today.AddDays(-2), AppointmentStatus.Completed, "B118");
        await EnsureRecordPlanAsync(context, patients[6], doctors[6], "Neuropathy assessment", "Burning sensation in feet, diabetes screening advised.", "Gabapentin", "100mg", "Take at bedtime for one week, then reassess symptoms.", DateTime.Today.AddDays(3), AppointmentStatus.Scheduled, "B312");
        await EnsureRecordPlanAsync(context, patients[7], doctors[4], "Emergency wound follow-up", "Laceration healing well. No signs of infection.", "Mupirocin", "2%", "Apply thin layer twice daily for 5 days.", DateTime.Today.AddDays(-1), AppointmentStatus.Completed, "E014");
        await EnsureRecordPlanAsync(context, patients[8], doctors[1], "Pediatric growth check", "Growth curve appropriate. Vaccination record reviewed.", "Vitamin D", "400 IU", "Take once daily with breakfast.", DateTime.Today.AddDays(6), AppointmentStatus.Scheduled, "C108");
        await EnsureRecordPlanAsync(context, patients[9], doctors[2], "Post-operative shoulder review", "Range of motion improving with physiotherapy.", "Diclofenac gel", "1%", "Apply to shoulder up to three times daily.", DateTime.Today.AddDays(8), AppointmentStatus.Scheduled, "D205");
        await EnsureRecordPlanAsync(context, patients[10], doctors[5], "Type 2 diabetes follow-up", "HbA1c improved. Continue lifestyle plan.", "Metformin", "500mg", "Take twice daily with meals.", DateTime.Today.AddDays(9), AppointmentStatus.Scheduled, "A116");
        await EnsureRecordPlanAsync(context, patients[11], doctors[6], "Migraine prevention consult", "Headache diary reviewed. Triggers include sleep deprivation.", "Propranolol", "40mg", "Take once daily unless dizziness occurs.", DateTime.Today.AddDays(10), AppointmentStatus.Scheduled, "B304");

        await EnsureAppointmentAsync(context, patients[0], doctors[5], DateTime.Today.AddDays(14).AddHours(10), AppointmentStatus.Scheduled, "A118", "Medication reconciliation and lab results review.");
        await EnsureAppointmentAsync(context, patients[4], doctors[4], DateTime.Today.AddDays(15).AddHours(9), AppointmentStatus.Scheduled, "E011", "Shortness of breath safety-net follow-up.");
        await EnsureAppointmentAsync(context, patients[10], doctors[0], DateTime.Today.AddDays(16).AddHours(13), AppointmentStatus.Scheduled, "A207", "Cardiology risk check after diabetes review.");
    }

    private static async Task<Department> EnsureDepartmentAsync(AppDbContext context, string name, string location, string phoneNumber, string headOfDepartment)
    {
        var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == name);
        if (department is not null)
        {
            return department;
        }

        department = new Department
        {
            Name = name,
            Location = location,
            PhoneNumber = phoneNumber,
            HeadOfDepartment = headOfDepartment
        };
        context.Departments.Add(department);
        return department;
    }

    private static async Task<Doctor> EnsureDoctorAsync(AppDbContext context, string firstName, string lastName, Gender gender, string specialty, string email, string phoneNumber, Department department)
    {
        var doctor = await context.Doctors
            .Include(d => d.Departments)
            .FirstOrDefaultAsync(d => d.Email == email);

        if (doctor is null)
        {
            doctor = new Doctor
            {
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                Specialty = specialty,
                Email = email,
                PhoneNumber = phoneNumber
            };
            context.Doctors.Add(doctor);
        }

        if (!doctor.Departments.Any(d => d.Name == department.Name))
        {
            doctor.Departments.Add(department);
        }

        return doctor;
    }

    private static async Task<Patient> EnsurePatientAsync(AppDbContext context, string firstName, string lastName, Gender gender, DateTime dateOfBirth, string email, string phoneNumber, string address)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Email == email);
        if (patient is not null)
        {
            return patient;
        }

        patient = new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            Email = email,
            PhoneNumber = phoneNumber,
            Address = address
        };
        context.Patients.Add(patient);
        return patient;
    }

    private static async Task EnsureRecordPlanAsync(AppDbContext context, Patient patient, Doctor doctor, string diagnosis, string notes, string medicationName, string dosage, string instructions, DateTime appointmentAt, AppointmentStatus status, string room)
    {
        var record = await context.MedicalRecords
            .Include(r => r.Prescriptions)
            .ThenInclude(p => p.Medications)
            .FirstOrDefaultAsync(r => r.PatientId == patient.Id && r.Diagnosis == diagnosis);

        if (record is null)
        {
            record = new MedicalRecord
            {
                PatientId = patient.Id,
                CreatedAt = DateTime.Today.AddDays(-14),
                Diagnosis = diagnosis,
                Notes = $"{DemoMarker} {notes}"
            };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();
        }

        var prescription = record.Prescriptions.FirstOrDefault(p => p.IssuedBy == doctor.LastName);
        if (prescription is null)
        {
            prescription = new Prescription
            {
                MedicalRecordId = record.Id,
                IssuedAt = DateTime.Today.AddDays(-7),
                IssuedBy = doctor.LastName
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();
        }

        if (!prescription.Medications.Any(m => m.Name == medicationName && m.Dosage == dosage))
        {
            context.Medications.Add(new Medication
            {
                PrescriptionId = prescription.Id,
                Name = medicationName,
                Dosage = dosage,
                Instructions = $"{DemoMarker} {instructions}"
            });
        }

        await EnsureAppointmentAsync(context, patient, doctor, appointmentAt, status, room, $"{diagnosis} follow-up.");
    }

    private static async Task EnsureAppointmentAsync(AppDbContext context, Patient patient, Doctor doctor, DateTime scheduledAt, AppointmentStatus status, string room, string notes)
    {
        var fullNotes = $"{DemoMarker} {notes}";
        var exists = await context.Appointments.AnyAsync(a =>
            a.PatientId == patient.Id &&
            a.DoctorId == doctor.Id &&
            a.Room == room &&
            a.Notes == fullNotes);

        if (exists)
        {
            return;
        }

        context.Appointments.Add(new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduledAt = scheduledAt,
            Status = status,
            Room = room,
            Notes = fullNotes
        });
    }
}
